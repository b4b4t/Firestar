using System;

namespace Firestar
{
    public class IpAddressInfo
    {
        #region Properties

        private const int MAX_REQUEST = 3;
        private const int BLOCKED_HOURS = 2;

        private readonly string _ipAddress = null;
        private int _counter = 0;
        private bool _blocked = false;
        private DateTime _lastDateRequest = DateTime.MinValue;
        private DateTime _blockedAtDate = DateTime.MinValue;

        public string IpAddress { get { return _ipAddress; } }
        public DateTime LastRequest { get { return _lastDateRequest; } }
        public DateTime BlockedAtDate { get { return _blockedAtDate; } }
        public bool Blocked { get { return _blocked; } }
        public int Counter { get { return _counter; } }

        #endregion

        public IpAddressInfo(string ipAddress)
        {
            _ipAddress = ipAddress;
        }

        #region Methods

        public void Increase()
        {
            _counter++;
            _lastDateRequest = DateTime.Now;

            if (_counter > MAX_REQUEST)
                _blocked = true;
            else
                _blocked = false;
        }

        public void Reset()
        {
            _counter = 0;
            _blocked = false;
            _blockedAtDate = DateTime.MinValue;
        }

        public void SetBlockedDate()
        {
            _blockedAtDate = DateTime.Now.AddHours(BLOCKED_HOURS);
        }

        #endregion
    }
}
