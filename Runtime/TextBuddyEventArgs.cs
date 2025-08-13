using System;
using UnityEngine;

namespace TextBuddy.Core
{

    public sealed class SDKInitializedEventArgs : EventArgs
    {
        public bool Status { get; }
        public ConnectError ErrorCode { get; }
        public string ErrorMessage { get; }

        public SDKInitializedEventArgs(bool status, ConnectError errorCode, string errorMessage)
        {
            Status = status;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }

    public sealed class UserSubscribedEventArgs : EventArgs
    {
        public string UserID { get; }
        public UserSubscribedEventArgs(string userID) { UserID = userID; }
    }

    public sealed class UserSubscribeFailedEventArgs : EventArgs
    {
        public int ErrorCode { get; }
        public string ErrorMessage { get; }
        public UserSubscribeFailedEventArgs(int errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
}