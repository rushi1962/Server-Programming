using CardGameTCPServer.TCP;

namespace CardGameTCPServer.Services
{
    public class AccountService
    {
        private static AccountService instance = new AccountService();

        public static AccountService Instance { get { return instance; } }

        Dictionary<int, PlayerAccount> accounts;

        private readonly object accountLock;

        private int nextAccountID;

        private AccountService() 
        {
            accounts = new Dictionary<int, PlayerAccount>();
            accountLock = new object();
            nextAccountID = 1;
        }

        public void CreateGuestAccount(out PlayerAccount account)
        {
            int accountID = nextAccountID++;

            account = new PlayerAccount();
            account.AccountID = accountID;

            lock (accountLock)
            {
                accounts.Add(accountID, account);
            }
        }
        public bool GetAccount(int accountID, out PlayerAccount account)
        {
            account = null;

            lock (accountLock)
            {
                if (accounts.ContainsKey(accountID))
                {
                    account = accounts[accountID];
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public void SaveAccount()
        {

        }
    }
}
