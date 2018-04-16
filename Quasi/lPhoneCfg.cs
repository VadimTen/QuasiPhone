using System;
using System.Collections.Generic;
using Sipek.Common.CallControl;
using Sipek.Common;
using System.Windows.Forms;

namespace iLLi.VOIP
{
    internal class rc_PhoneCfg : IConfiguratorInterface
    {
        List<IAccount> v_slAccList = new List<IAccount>();

        internal rc_PhoneCfg()
        {
            v_slAccList.Add(new rc_AccountCfg());
        }

        #region IConfiguratorInterface Members
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

        public List<IAccount> Accounts
        {
            get
            {
                return v_slAccList;
            }
        }

        public bool CFBFlag
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public string CFBNumber
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public bool CFNRFlag
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public string CFNRNumber
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public bool CFUFlag
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public string CFUNumber
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public List<String> CodecList
        {
            get
            {
                List<String> slCodecs = new List<String>();
                slCodecs.Add("G.722");
                return slCodecs;
            }
            set
            {
            }
        }
        private bool _DNDFlag;
        public bool DNDFlag
        {
            get
            {
                return _DNDFlag;
            }
            set
            {
                _DNDFlag = value;
            }
        }

        public int DefaultAccountIndex
        {
            get
            {
                return 0;
            }
        }

        public bool IsNull
        {
            get
            {
                return false;
            }
        }

        public bool PublishEnabled
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public int SIPPort
        {
            get
            {
                return 5070;
            }
            set
            {
            }
        }

        public void Save()
        {
            // TODO
        }

        #endregion
    } // class rc_PhoneCfg

} // namespace iLLi.VOIP