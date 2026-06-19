using UnityEngine;
using UnityEngine.UI;

namespace CozyStroll.Interaction
{
    /// <summary>
    /// Finds the nearest <see cref="Interactable"/> in range, shows its prompt,
    /// and triggers it on E. Keeps things calm: only one prompt at a time.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [Tooltip("Label used to show 'Nhấn E để ...'. Its parent is toggled on/off.")]
        public Text promptLabel;

        public KeyCode interactKey = KeyCode.E;

        private static readonly Collider[] _hits = new Collider[16];

        private void Update()
        {
            var target = FindNearest();

            // Toggle the prompt panel (the label's parent image).
            if (promptLabel != null)
            {
                var panel = promptLabel.transform.parent != null
                    ? promptLabel.transform.parent.gameObject
                    : promptLabel.gameObject;

                if (target != null)
                {
                    if (!panel.activeSelf) panel.SetActive(true);
                    promptLabel.text = target.Prompt;
                }
                else if (panel.activeSelf)
                {
                    panel.SetActive(false);
                }
            }

            if (target != null && Input.GetKeyDown(interactKey))
                target.Interact();
        }

        private Interactable FindNearest()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, 3f, _hits);
            Interactable best = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var it = _hits[i].GetComponentInParent<Interactable>();
                if (it == null || !it.CanInteract) continue;

                float sqr = (it.transform.position - transform.position).sqrMagnitude;
                if (sqr <= it.radius * it.radius && sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = it;
                }
            }
            return best;
        }
    }
}
