using System;

namespace Firestar
{
    public class IpAddress
    {
        private const int MAX_REQUEST = 3;

        private readonly object ipLock = new object();
        private readonly string _ipAddress;
        private DateTime _date;
        private int _counter = 1;
        private bool _blocked;

        public IpAddress(string ipAddress)
        {
            _ipAddress = ipAddress;
        }

        public void ResetDate()
        {
            lock (ipLock)
            {
                _date = DateTime.Now;
            }
        }

        public DateTime GetDate()
        {
            return _date;
        }

        public bool IsBlocked()
        {
            return _blocked;
        }

        public bool Increase()
        {
            lock (ipLock)
            {
                _counter++;
                _date = DateTime.Now;

                if (_counter > MAX_REQUEST)
                {
                    _blocked = true;
                }
                else
                {
                    _blocked = false;
                }
            }

            return _counter == MAX_REQUEST + 1;
        }

        public void Reset()
        {
            lock (ipLock)
            {
                _counter = 0;
                _blocked = false;
            }
        }
    }
}
