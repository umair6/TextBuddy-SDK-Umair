using UnityEngine;

namespace TextBuddy.Core
{
    /// <summary>
    /// Holds basic sign-up information passed via deep links or user action.
    /// </summary>
    public struct SignUpInfo
    {
        public string Action;
        public string GameID;

        /// <summary>
        /// Returns a formatted string representation of the sign-up info.
        /// </summary>
        public override string ToString() =>
            $"Action: {Action}\nGameID: {GameID}";
    }
}
