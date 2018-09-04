using MonoXEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XEditor;

namespace MyGame
{
    /// <summary>
    /// Base class for all enemies
    /// </summary>
    public abstract class Enemy
    {
        #region Static properties
        /// <summary>
        /// Once intialised updates for all enemies will run
        /// </summary>
        private static bool InitialisedUpdates = false;

        /// <summary>
        /// This will automatically fill and unfill as enemies are created and die.
        /// </summary>
        public static List<Enemy> EnemyList = new List<Enemy>();
        #endregion
        
        #region Default constructor
        public Enemy(EntityInfo entityInfo)
        {
            // Set data
            EntityInfo = entityInfo;

            // Add to enemy list
            EnemyList.Add(this);

            // Initiate updates
            if(!InitialisedUpdates)
                StaticCoroutines.CoroutineHelper.Always(() => { Update(); });
            
        }
        #endregion

        #region Properties
        /// <summary>
        /// Maximum HP
        /// </summary>
        private int? MaxHP = null;

        /// <summary>
        /// Current HP
        /// </summary>
        public int HP {
            get => hP;
            set {
                if (MaxHP == null)
                    MaxHP = value;

                hP = value.Between(0, (int)MaxHP);
                CheckAlive();
            }
        }
        private int hP;

        /// <summary>
        /// Alias
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Entity instance
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Data
        /// </summary>
        public EntityInfo EntityInfo { get; set; }
        #endregion

        #region Public methods
        public virtual void Update()
        {
            CheckAlive();
        }

        /// <summary>
        /// Checks if enemy is alive (By default this just checks whether HP > 0)
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckAlive()
        {
            return HP > 0;
        }

        /// <summary>
        /// Destroys all associated with this instance, remember to nullify any reference to this Enemy instance
        /// </summary>
        public virtual void Die()
        {
            EnemyList.Remove(this);
            Entity.Destroy();
        }
        #endregion
    }
}
