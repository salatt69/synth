using System;
using UnityEngine;

namespace RoR2.UI
{
    // Token: 0x020015B1 RID: 5553
    [DisallowMultipleComponent]
    public class HudElement : MonoBehaviour
    {
        // Token: 0x17000BB2 RID: 2994
        // (get) Token: 0x0600850A RID: 34058 RVA: 0x0023C1F0 File Offset: 0x0023A3F0
        // (set) Token: 0x0600850B RID: 34059 RVA: 0x0023C1F8 File Offset: 0x0023A3F8
        public Component hud
        {
            get
            {
                return this._hud;
            }
            set
            {
                this._hud = value;
            }
        }

        // Token: 0x17000BB3 RID: 2995
        // (get) Token: 0x0600850C RID: 34060 RVA: 0x0023C227 File Offset: 0x0023A427
        // (set) Token: 0x0600850D RID: 34061 RVA: 0x0023C22F File Offset: 0x0023A42F
        public GameObject targetBodyObject
        {
            get
            {
                return this._targetBodyObject;
            }
            set
            {
                this._targetBodyObject = value;
                if (this._targetBodyObject)
                {
                    this._targetCharacterBody = this._targetBodyObject.GetComponent<Component>();
                }
            }
        }

        // Token: 0x17000BB4 RID: 2996
        // (get) Token: 0x0600850E RID: 34062 RVA: 0x0023C256 File Offset: 0x0023A456
        // (set) Token: 0x0600850F RID: 34063 RVA: 0x0023C25E File Offset: 0x0023A45E
        public Component targetCharacterBody
        {
            get
            {
                return this._targetCharacterBody;
            }
            set
            {
                this._targetCharacterBody = value;
                if (this.targetCharacterBody)
                {
                    this._targetBodyObject = this.targetCharacterBody.gameObject;
                }
            }
        }

        // Token: 0x0400839C RID: 33692
        private Component _hud;

        // Token: 0x0400839D RID: 33693
        private GameObject _targetBodyObject;

        // Token: 0x0400839E RID: 33694
        private Component _targetCharacterBody;
    }
}
