using System;
using Convai.Scripts.Runtime.Core;
using Convai.Scripts.Runtime.UI;
using UnityEngine;


namespace Convai.Scripts.Runtime.Addons
{
    /// <summary>
    ///     Controls player input to trigger a notification if there is no active NPC available for conversation.
    /// </summary>
    public class ActiveNPCChecker : MonoBehaviour
    {

    private void Update()
    {
        if (ConvaiInputManager.Instance.WasTalkKeyPressed())
        {
            if (ConvaiNPCManager.Instance.activeConvaiNPC == null)
                NotificationSystemHandler.Instance.NotificationRequest(NotificationType.NotCloseEnoughForConversation);
        }
    }

    }
}