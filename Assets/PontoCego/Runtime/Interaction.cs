using UnityEngine;
using UnityEngine.SceneManagement;

namespace PontoCego
{
    public interface IInteractable
    {
        string Prompt { get; }
        void Interact();
    }

    /// <summary>Explicit target selection, distance and line-of-sight checks; no trigger spam.</summary>
    public sealed class Interaction : MonoBehaviour
    {
        [Min(.1f)] public float range = 1.65f;
        public string Prompt => target != null ? target.Prompt : "";
        public string Message { get; private set; } = "";
        public float MessageUntil { get; private set; }
        readonly Collider2D[] results = new Collider2D[128];
        IInteractable target;
        PlayerInputReader input;
        void Awake() { input = GetComponent<PlayerInputReader>(); }
        void Update()
        {
            if (input.Paused||MissionIntro2D.Showing) return;
            var dialogue=GetComponent<StoryDialogue>();
            if(dialogue && dialogue.Active)
            {
                target=null;
                if(input.ConsumeInteract())dialogue.Advance();
                return;
            }
            if(input.GameplayLocked)return;
            if(DriveableCar.Current){target=null;if(input.ConsumeInteract())DriveableCar.Current.Exit();return;}
            target = null;
            float best = float.MaxValue;
            int count = Physics2D.OverlapCircle(transform.position, Mathf.Max(range,2.05f),
                new ContactFilter2D { useLayerMask = true, layerMask = LayerMask.GetMask("Interactable"), useTriggers = true }, results);
            for (int i = 0; i < count; ++i)
            {
                var item = results[i].GetComponentInParent<IInteractable>();
                if (item == null) continue;
                var level=FindAnyObjectByType<LevelProgress>();
                var story=results[i].GetComponent<StoryInteractable>();if(story&&story.requiredStep>=0&&story.requiredStep!=level.Step)continue;
                var puzzle=results[i].GetComponent<EvidencePuzzle>();if(puzzle&&puzzle.requiredStep!=level.Step)continue;
                Vector2 delta = results[i].transform.position - transform.position;
                bool pickup=item is PcCollectible||item is FoodPickup||item is GoldenQuestPickup;if(delta.magnitude>(pickup?2.05f:range))continue;float score=delta.sqrMagnitude-(pickup?4:0);if (score >= best) continue;
                var obstruction=Physics2D.Raycast(transform.position, delta.normalized, delta.magnitude, LayerMask.GetMask("World"));if(obstruction.collider&&!obstruction.collider.transform.IsChildOf(results[i].transform))continue;
                best = score; target = item;
            }
            if (input.ConsumeInteract()) target?.Interact();
        }
        public void Say(string text) { Message = text; MessageUntil = Time.unscaledTime + 6f; }
    }

}
