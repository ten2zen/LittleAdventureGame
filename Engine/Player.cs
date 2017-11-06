using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Engine
{
    public class Player : LivingCreature
    {
        public int Gold { get; set; }
        public int ExperiencePoints { get; set; }

        public int Level
        {
            get { return ((ExperiencePoints / 100) + 1); }
        }

        public Location CurrentLocation { get; set; }
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }

        public Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints) : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }

        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.ItemRequiredToEnter == null)
            {
                // Для доступа к локации не нужно иметь какого-либо предмета
                return true;
            }

            // Проверить, имеется ли у игрока необходимый предмет
            return Inventory.Exists(ii => ii.Details.ID == location.ItemRequiredToEnter.ID);
        }

        public bool HasThisQuest(Quest quest)
        {
            return Quests.Exists(pq => pq.Details.ID == quest.ID);
        }

        public bool CompletedThisQuest(Quest quest)
        {
            foreach (PlayerQuest playerQuest in Quests)
            {
                if (playerQuest.Details.ID == quest.ID)
                {
                    return playerQuest.IsCompleted;
                }
            }

            return false;
        }

        public bool HasAllQuestCompletionItems(Quest quest)
        {
            // Проверить, имеются ли необходимые для выполнения квеста предметы
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                if (!Inventory.Exists(ii => ii.Details.ID == qci.Details.ID &&
                                      ii.Quantity >= qci.Quantity))
                {
                    return false;
                }
            }

            // Необходимые предметы и необходимое их количество имеется для выполнения квеста
            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == qci.Details.ID);

                if (item != null)
                {
                    // Вычесть количество предметов, которое было необходимо для завершения задания
                    item.Quantity -= qci.Quantity;
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

            if (item == null)
            {
                // Добавить предмет в инвентарь и увеличить количество предметов на один
                Inventory.Add(new InventoryItem(itemToAdd, 1));
            }
            else
            {
                // Предмет уже имеется, увеличить количество на один
                item.Quantity++;
            }
        }

        public void MarkQuestCompleted(Quest quest)
        {
            // Найти квест в списке заданий игрока
            PlayerQuest playerQuest = Quests.SingleOrDefault(pq => pq.Details.ID == quest.ID);

            if (playerQuest != null)
            {
                playerQuest.IsCompleted = true;
            }
        }
    }
}