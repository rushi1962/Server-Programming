#!/usr/bin/env python3
"""Test client for CardGameTCPServer, standing in for the Unity client.

Each bot connects, logs in as a guest, asks for matchmaking and plays its turns
(attack when it has the mana, otherwise mana boost) until the match ends.

Usage:
  python3 cardgame_bot.py 192.168.252.2              # 2 bots play one match
  python3 cardgame_bot.py 192.168.252.2 --bots 4     # 2 matches at once
  python3 cardgame_bot.py 192.168.252.2 --port 7777 --timeout 120

Exit code: 0 if every bot finished a match, 1 otherwise, so it also works as a smoke test.
"""
import argparse
import json
import socket
import struct
import sys
import threading
import time

# PacketType
SYSTEM, LOBBY, GAME = 1, 2, 3
# SystemPacketTypes
(MATCHMAKING_REQUESTED, CLIENT_NAME, CLIENT_UUID, LEAVE_MATCH, HEARTBEAT, RECONNECTION_TOKEN,
 RECONNECTION_SUCCESS, SHUTDOWN_COUNTDOWN, SHUTDOWN, LOGIN_GUEST, LOGIN_WITH_ACCOUNT_ID,
 LOGIN_SUCCESS, LOGIN_FAILED) = range(1, 14)
# GamePacketTypes
GAME_ACTION, GAME_STATE_UPDATE, GAME_STARTED = 1, 2, 3
# GameActionTypes
ATTACK, HEAL, MANA_BOOST = 1, 2, 3
ACTION_NAMES = {ATTACK: "ATTACK", HEAL: "HEAL", MANA_BOOST: "MANA BOOST"}

HEARTBEAT_SECONDS = 2.5  # same as the Unity client's HeartbeatGapInSeconds

print_lock = threading.Lock()


class Bot:
    def __init__(self, name, host, port):
        self.name = name
        self.host = host
        self.port = port
        self.sock = None
        self.send_lock = threading.Lock()
        self.client_id = None
        self.logged_in = threading.Event()
        self.finished = threading.Event()
        self.result = None
        self.stop = False

    def log(self, msg):
        with print_lock:
            print(f"[{time.strftime('%H:%M:%S')}] [{self.name}] {msg}", flush=True)

    # ---- sending: every value is a little-endian int32, like BitConverter on the server ----
    def send_ints(self, *values):
        with self.send_lock:
            self.sock.sendall(struct.pack("<" + "i" * len(values), *values))

    # ---- receiving ----
    def recv_exact(self, n):
        data = b""
        while len(data) < n:
            chunk = self.sock.recv(n - len(data))
            if not chunk:
                raise ConnectionError("server closed the connection")
            data += chunk
        return data

    def read_int(self):
        return struct.unpack("<i", self.recv_exact(4))[0]

    def read_string(self):
        return self.recv_exact(self.read_int()).decode("utf-8")

    def reader(self):
        try:
            while not self.stop:
                packet_type = self.read_int()
                sub_type = self.read_int()
                if packet_type == SYSTEM:
                    self.handle_system(sub_type)
                elif packet_type == GAME:
                    self.handle_game(sub_type)
                else:
                    self.log(f"unknown packet type {packet_type}/{sub_type}, stopping")
                    return
        except (ConnectionError, OSError) as e:
            if not self.stop:
                self.log(f"disconnected: {e}")
        finally:
            self.finished.set()

    def handle_system(self, sub_type):
        if sub_type == CLIENT_UUID:
            self.client_id = self.read_int()
            self.log(f"server gave me client ID {self.client_id}")
        elif sub_type == CLIENT_NAME:
            self.log(f"server named me {self.read_string()}")
        elif sub_type == RECONNECTION_TOKEN:
            self.read_string()  # not needed for this test
        elif sub_type == LOGIN_SUCCESS:
            self.log(f"login OK, account ID {self.read_int()}")
            self.logged_in.set()
        elif sub_type == LOGIN_FAILED:
            self.log(f"server also sent LOGIN FAILED: '{self.read_string()}'")
        elif sub_type == SHUTDOWN_COUNTDOWN:
            self.log(f"server shutting down in {self.read_int()}s")
        elif sub_type == SHUTDOWN:
            self.log("server shut down")
            self.stop = True
        elif sub_type == RECONNECTION_SUCCESS:
            self.log("reconnection success")
        else:
            raise ConnectionError(f"unexpected system packet {sub_type}")

    def handle_game(self, sub_type):
        if sub_type == GAME_STARTED:
            self.log("match started")
        elif sub_type == GAME_STATE_UPDATE:
            self.handle_state(json.loads(self.read_string()))
        else:
            raise ConnectionError(f"unexpected game packet {sub_type}")

    def handle_state(self, state):
        p1, p2 = state["PlayerState_1"], state["PlayerState_2"]
        me, them = (p1, p2) if p1["PlayerID"] == self.client_id else (p2, p1)
        if state["IsGameOver"]:
            winner = state["GameWinnerID"]
            self.result = "WON" if winner == self.client_id else ("DRAW" if winner == 0 else "LOST")
            self.log(f"match over: {self.result}  (me HP {me['PlayerHealth']}, them HP {them['PlayerHealth']})")
            self.stop = True
            return
        if state["GameTurnPlayerID"] != self.client_id:
            return
        action = ATTACK if me["PlayerMana"] >= me["PlayerAttackCost"] else MANA_BOOST
        self.log(f"my turn: me HP {me['PlayerHealth']} MP {me['PlayerMana']} | "
                 f"them HP {them['PlayerHealth']} MP {them['PlayerMana']}  ->  {ACTION_NAMES[action]}")
        time.sleep(0.3)  # so a human can follow the log
        self.send_ints(GAME, GAME_ACTION, action)

    def heartbeat(self):
        while not self.stop:
            time.sleep(HEARTBEAT_SECONDS)
            try:
                self.send_ints(SYSTEM, HEARTBEAT)
            except OSError:
                return

    def run(self):
        self.sock = socket.create_connection((self.host, self.port), timeout=10)
        self.sock.settimeout(None)
        self.log(f"connected to {self.host}:{self.port}")
        threading.Thread(target=self.reader, daemon=True).start()
        threading.Thread(target=self.heartbeat, daemon=True).start()

        self.send_ints(SYSTEM, LOGIN_GUEST)
        if not self.logged_in.wait(10):
            self.log("no login answer within 10s")
            return
        self.send_ints(SYSTEM, MATCHMAKING_REQUESTED)
        self.log("asked for matchmaking")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("host")
    parser.add_argument("--port", type=int, default=7777)
    parser.add_argument("--bots", type=int, default=2, help="number of bots (2 per match)")
    parser.add_argument("--timeout", type=int, default=120, help="give up after this many seconds")
    args = parser.parse_args()

    bots = [Bot(f"bot{i + 1}", args.host, args.port) for i in range(args.bots)]
    for bot in bots:
        try:
            bot.run()
        except OSError as e:
            bot.log(f"could not connect: {e}")
            bot.finished.set()
        time.sleep(0.5)

    deadline = time.time() + args.timeout
    for bot in bots:
        bot.finished.wait(max(0, deadline - time.time()))

    for bot in bots:
        bot.stop = True
        if bot.sock:
            bot.sock.close()

    results = {bot.name: bot.result for bot in bots}
    print(f"\nResults: {results}")
    sys.exit(0 if all(results.values()) else 1)


if __name__ == "__main__":
    main()
