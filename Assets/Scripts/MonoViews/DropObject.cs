using GamePush;
using System;
using UnityEngine;

namespace Code.DropLogic
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class DropObject : DropBase
    {
        [SerializeField, Range(0, 3)]
        private float _knockbackRadius;
        private float _loseLine;

        public float KnockbackRadius => _knockbackRadius;

        private void Start()
        {
#if UNITY_EDITOR
            _loseLine = Constants.LoseThreshold;
#else
            _loseLine = GP_Variables.GetFloat("LoseThreshold");
#endif
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var overThreshold = transform.position.y > _loseLine;
            if (overThreshold && collision.gameObject.GetComponent<DropObject>())
                GameEventSystem.Send(new GameControlEvent(GameAction.Lose, false));

            if (RB.gravityScale == 0 || CollisionsIgnored)
                return;

            _collisionsIgnored = true;
            CheckRankCollisions(collision);
        }

        private void CheckRankCollisions(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent(out DropObject drop) && drop.Rank == _rank)
            {
                if (drop.transform.position.y <= transform.position.y) //the upper one calls for merge
                {
                    drop.CollisionsIgnored = CollisionsIgnored;
                    InitiateMergeWith(drop);
                }
            }
            else
            {
                _collisionsIgnored = false;
            }
        }

        private void Register(GameControlEvent @event)
        {
            bool shouldRegister = @event.ActionToDo != GameAction.Pause && @event.ActionToDo != GameAction.Lose;
            if (shouldRegister && RB.gravityScale != 0)
                GameEventSystem.Send(new ManageDropEvent(this, @event.RestartWithRetry, withEffects: false));
        }

        private void ReturnToPool(BombEvent @event)
        {
            if(@event.Rank == _rank && RB.gravityScale != 0)
                GameEventSystem.Send(new ManageDropEvent(this, true, withEffects: true));
        }

        protected override void OnDrop() => RB.CauseKnockback(this);

        protected override void ActivateSubscribtions()
        {
            GameEventSystem.Subscribe<GameControlEvent>(Register);
            GameEventSystem.Subscribe<BombEvent>(ReturnToPool);
        }

        protected override void CancelSubscribtions()
        {
            GameEventSystem.UnSubscribe<GameControlEvent>(Register);
            GameEventSystem.UnSubscribe<BombEvent>(ReturnToPool);
        }
    }
}