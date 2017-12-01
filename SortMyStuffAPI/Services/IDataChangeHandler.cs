using Microsoft.EntityFrameworkCore;
using SortMyStuffAPI.Models;
using System;

namespace SortMyStuffAPI.Services
{
    public interface IDataChangeHanlder
    {
        bool AnyRegistration();

        void OnDataChanged(params DataChangeEventArgs[] changes);
    }

    public interface IDataChangeSender
    {
        void Subscribe(EventHandler<DataChangeEventArgs> handleDataChange);

        void Unsubscribe(EventHandler<DataChangeEventArgs> handleDataChange);

        void UnsubscribeAll();
    }

    public class DataChangeEventArgs : EventArgs
    {
        public object Resource { get; private set; }
        public EntityState State { get; private set; }

        public DataChangeEventArgs(
            object resource,
            EntityState state)
        {
            Resource = resource;
            State = state;
        }
    }
}
