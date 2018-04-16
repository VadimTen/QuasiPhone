using System.Collections.Generic;

using Sipek.Common;

namespace iLLi.VOIP
{

    class rc_AccountCfg : IAccount
    {

        #region IAccount Members
        private string _AccountName;
        public string AccountName
        {
            get
            {
                return _AccountName;
            }
            set
            {
                _AccountName = value;
            }
        }

        private string _DisplayName;
        public string DisplayName
        {
            get
            {
                return _DisplayName;
            }
            set
            {
                _DisplayName = value;
            }
        }

        public string DomainName
        {
            get
            {
                return "*";
            }
            set
            {
            }
        }
        private bool _Enabled;

        public bool Enabled
        {
            get
            {
                return _Enabled;
            }
            set
            {
                _Enabled = value;
            }
        }

        private string _HostName;
        public string HostName
        {
            get
            {
                return _HostName;
            }
            set
            {
                _HostName = value;
            }
        }
        private string _Id;

        public string Id
        {
            get
            {
                return _Id;
            }
            set
            {
                _Id = value;
            }
        }

        private int _Index;
        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                _Index = value;
            }
        }
        private string _Password;
        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                _Password = value;
            }
        }
        public string _ProxyAddress;
        public string ProxyAddress
        {
            get
            {
                return _ProxyAddress;
            }
            set
            {
                _ProxyAddress = value;
            }
        }
        private int _RegState;
        public int RegState
        {
            get
            {
                return _RegState;
            }
            set
            {
                _RegState = value;

            }
        }

        public ETransportMode TransportMode
        {
            get
            {
                return ETransportMode.TM_UDP;
            }
            set
            {
            }
        }
        private string _UserName;
        public string UserName
        {
            get
            {
                return _UserName;
            }
            set
            {
                _UserName = value;
            }
        }
        private bool _AAFlag;
        public bool AAFlag
        {
            get
            {
                return _AAFlag;
            }
            set 
            {
                _AAFlag = value; 
            }
        }
        #endregion
    } // class rc_AccountCfg

} // namespace iLLi.VOIP