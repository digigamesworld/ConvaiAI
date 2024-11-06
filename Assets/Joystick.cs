using UnityEngine;
using Image = UnityEngine.UI.Image;
using UnityEngine.EventSystems;

namespace Inputs
{
    public enum AxisOptions { Both, Horizontal, Vertical }

    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public float Horizontal => SnapX ? SnapFloat(input.x, AxisOptions.Horizontal) : input.x;
        public float Vertical => SnapY ? SnapFloat(input.y, AxisOptions.Vertical) : input.y; 
        public Vector2 Direction => new Vector2(Horizontal, Vertical); 

        public float HandleRange
        {
            get => handleRange; 
            set => handleRange = Mathf.Abs(value); 
        }
        public float DeadZone
        {
            get => deadZone;
            set => deadZone = Mathf.Abs(value); 
        }

        [SerializeField] private float handleRange = 1;
        [SerializeField] private float deadZone = 0;
        [field: SerializeField] public AxisOptions AxisOptions { private set; get; } = AxisOptions.Both;
        [field: SerializeField] public bool SnapX { private set; get; } = false;
        [field: SerializeField] public bool SnapY { private set; get; } = false;

        [SerializeField] private RectTransform background = null;
        [SerializeField] private RectTransform handle = null;
        private Image backgroundImg = null;
        private Image handleImg = null;

        private RectTransform baseRect = null;

        private Canvas canvas;
        private Camera cam;

        private Vector2 input = Vector2.zero;
        private Color color;

        protected virtual void Start()
        {
            HandleRange = handleRange;
            DeadZone = deadZone;
            baseRect = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                Debug.LogError("The Joystick is not placed inside a canvas");

            Vector2 center = new Vector2(0.5f, 0.5f);
            background.pivot = center;
            handle.anchorMin = center;
            handle.anchorMax = center;
            handle.pivot = center;
            handle.anchoredPosition = Vector2.zero;

            backgroundImg = background.GetComponent<Image>();
            handleImg = handle.GetComponent<Image>();
            color = backgroundImg.color;
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            cam = null;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                cam = canvas.worldCamera;

            Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
            Vector2 radius = background.sizeDelta / 2;
            input = (eventData.position - position) / (radius * canvas.scaleFactor);
            FormatInput();
            HandleInput(input.magnitude, input.normalized, radius, cam);
            handle.anchoredPosition = input * radius * handleRange;
        }

        protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
        {
            if (magnitude > deadZone)
            {
                if (magnitude > 1)
                    input = normalised;
            }
            else
                input = Vector2.zero;
        }

        private void FormatInput()
        {
            switch (AxisOptions)
            {
                case AxisOptions.Horizontal:
                    input = new Vector2(input.x, 0f);
                    break;
                case AxisOptions.Vertical:
                    input = new Vector2(0f, input.y);
                    break;
            }             
        }

        private float SnapFloat(float value, AxisOptions snapAxis)
        {
            if (value == 0)
                return value;

            if (AxisOptions == AxisOptions.Both)
            {
                float angle = Vector2.Angle(input, Vector2.up);

                switch (snapAxis)
                {
                    case AxisOptions.Horizontal:
                        return angle < 22.5f || angle > 157.5f ? 0 : value > 0 ? 1 : -1;
                    case AxisOptions.Vertical:
                        return angle > 67.5f && angle < 112.5f ? 0 : value > 0 ? 1 : -1;
                    default:
                        return value;
                }
            }

            return value > 0 ? 1 : value < 0 ? -1 : 0;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            input = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
        }

        protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            Vector2 localPoint = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, screenPosition, cam, out localPoint))
            {
                Vector2 pivotOffset = baseRect.pivot * baseRect.sizeDelta;
                return localPoint - (background.anchorMax * baseRect.sizeDelta) + pivotOffset;
            }
            return Vector2.zero;
        }
    }

}