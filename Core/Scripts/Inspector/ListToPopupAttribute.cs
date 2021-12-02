using System;
using System.Collections.Generic;
using UnityEngine;

namespace Didimo.Core.Inspector
{
    public class ListToPopupAttribute : PropertyAttribute
    {
        private Type listPopupType;

        public interface IListToPopup
        {
            List<string> ListToPopupGetValues();
        }

    }
}