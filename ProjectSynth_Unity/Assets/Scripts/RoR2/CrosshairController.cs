using System;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;

namespace RoR2.UI
{
    // Token: 0x0200156E RID: 5486
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(HudElement))]
    public class CrosshairController : MonoBehaviour
    {
        // Token: 0x17000B98 RID: 2968
        // (get) Token: 0x0600836C RID: 33644 RVA: 0x00232B81 File Offset: 0x00230D81
        // (set) Token: 0x0600836D RID: 33645 RVA: 0x00232B89 File Offset: 0x00230D89
        public RectTransform rectTransform { get; private set; }

        // Token: 0x17000B99 RID: 2969
        // (get) Token: 0x0600836E RID: 33646 RVA: 0x00232B92 File Offset: 0x00230D92
        // (set) Token: 0x0600836F RID: 33647 RVA: 0x00232B9A File Offset: 0x00230D9A
        public HudElement hudElement { get; private set; }

        // Token: 0x06008370 RID: 33648 RVA: 0x00232BA3 File Offset: 0x00230DA3
        private void Awake()
        {
            this.rectTransform = base.GetComponent<RectTransform>();
            this.hudElement = base.GetComponent<HudElement>();
            this.SetCrosshairSpread();
            this.SetSkillStockDisplays();
        }

        // Token: 0x06008371 RID: 33649 RVA: 0x00232BCC File Offset: 0x00230DCC
        private void SetCrosshairSpread()
        {
        }

        // Token: 0x06008372 RID: 33650 RVA: 0x00232CA0 File Offset: 0x00230EA0
        private void SetSkillStockDisplays()
        {
        }

        // Token: 0x06008373 RID: 33651 RVA: 0x00232D69 File Offset: 0x00230F69
        private void LateUpdate()
        {
        }

        // Token: 0x0400818E RID: 33166
        public CrosshairController.SpritePosition[] spriteSpreadPositions;

        // Token: 0x0400818F RID: 33167
        public CrosshairController.SkillStockSpriteDisplay[] skillStockSpriteDisplays;

        // Token: 0x04008190 RID: 33168
        public RawImage[] remapSprites;

        // Token: 0x04008191 RID: 33169
        public float minSpreadAlpha;

        // Token: 0x04008192 RID: 33170
        public float maxSpreadAlpha;

        // Token: 0x04008193 RID: 33171
        [Tooltip("The angle the crosshair represents when alpha = 1")]
        public float maxSpreadAngle;

        // Token: 0x04008194 RID: 33172
        private MaterialPropertyBlock _propBlock;

        // Token: 0x0200156F RID: 5487
        [Serializable]
        public struct SpritePosition
        {
            // Token: 0x04008195 RID: 33173
            public RectTransform target;

            // Token: 0x04008196 RID: 33174
            public Vector3 zeroPosition;

            // Token: 0x04008197 RID: 33175
            public Vector3 onePosition;
        }

        // Token: 0x02001570 RID: 5488
        [Serializable]
        public struct SkillStockSpriteDisplay
        {
            // Token: 0x04008198 RID: 33176
            public GameObject target;

            // Token: 0x04008199 RID: 33177
            public SkillSlot skillSlot;

            // Token: 0x0400819A RID: 33178
            public Component requiredSkillDef;

            // Token: 0x0400819B RID: 33179
            public int minimumStockCountToBeValid;

            // Token: 0x0400819C RID: 33180
            public int maximumStockCountToBeValid;
        }
    }
}
