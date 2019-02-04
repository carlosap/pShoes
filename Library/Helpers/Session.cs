using MadServ.Core.Interfaces;

namespace Library.Helpers
{
    public class Session
    {
        private ICore _core;

        public Session(ICore core)
        {
            _core = core;
        }

        public bool Exists(string key)
        {
            return _core.SessionDataStorage.Exists(key);
        }

        public bool NotExists(string key)
        {
            return !Exists(key);
        }

        public void Add<T>(string key, T dataObject)
        {
            _core.SessionDataStorage.Add<T>(key, dataObject);
        }

        public T Get<T>(string key)
        {
            return _core.SessionDataStorage.Get<T>(key);
        }

        public void Remove(string key)
        {
            _core.SessionDataStorage.Remove(key);
        }
    }
}
