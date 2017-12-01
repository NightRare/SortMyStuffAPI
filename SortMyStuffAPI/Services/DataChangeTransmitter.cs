using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Services
{
    public class DataChangeTransmitter :
        IDataChangeHanlder, IDataChangeSender
    {
        private event EventHandler<DataChangeEventArgs> HandleDataChanges;

        private static DataChangeTransmitter _intance = new DataChangeTransmitter();

        public static DataChangeTransmitter GetInstance()
        {
            return _intance;
        }

        public bool AnyRegistration()
        {
            return HandleDataChanges != null;
        }

        public void OnDataChanged(params DataChangeEventArgs[] changes)
        {
            foreach (var change in changes)
            {
                Delegate[] list = HandleDataChanges.GetInvocationList();
                foreach (var handleChange in list)
                {
                    new Task(() =>
                    {
                        try
                        {
                            (handleChange as EventHandler<DataChangeEventArgs>)
                            .Invoke(this, change);
                        }
                        catch (Exception) { }
                    }).Start();
                }
            }
        }

        public void Subscribe(EventHandler<DataChangeEventArgs> handleDataChange)
        {
            HandleDataChanges += handleDataChange;
        }

        public void Unsubscribe(EventHandler<DataChangeEventArgs> handleDataChange)
        {
            HandleDataChanges -= handleDataChange;
        }

        public void UnsubscribeAll()
        {
            HandleDataChanges = null;
        }

        private DataChangeTransmitter() { }
    }
}
