using UnityEngine;

namespace Code.DropLogic
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class BombView : DropBase
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_collisionsIgnored)
                return;

            _collisionsIgnored = true;
            if (collision.gameObject.TryGetComponent(out DropObject drop))
            {
                GameEventSystem.Send(new BombEvent(drop.Rank));
                GameEventSystem.Send(new ManageDropEvent(this, returnToPool: true, withEffects: false));
            }
            else
            {
                GameEventSystem.Send(new ManageDropEvent(this, returnToPool: true, withEffects: true));
            }
            GameEventSystem.Send(new SoundEvent(SoundType.Poof, true));
        }

        protected override void OnDrop()
        {
            base.OnDrop();
            MergeCounter.BombUse();
        }
    }
}