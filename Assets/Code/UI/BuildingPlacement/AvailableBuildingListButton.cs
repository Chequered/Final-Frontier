using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using FinalFrontier.Managers;
using FinalFrontier.Entities;

namespace FinalFrontier
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
