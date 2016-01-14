using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using EndlessExpedition.Managers;
using EndlessExpedition.Entities;

namespace EndlessExpedition
{
    namespace UI
    {
        public class AvailableBuildingButton : MonoBehaviour
        {
            public Building building;

            public void OnClick()
            {
                ManagerInstance.Get<UIManager>().buildingPlacementScreen.OnBuildingSwitch(building);
            }
        }
    }
}
