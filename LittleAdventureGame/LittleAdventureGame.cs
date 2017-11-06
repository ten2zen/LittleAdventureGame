using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Engine;

namespace LittleAdventureGame
{
    public partial class LittleAdventure : Form
    {
        private Player player;
        private Monster currentMonster;
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        public LittleAdventure()
        {
            InitializeComponent();

            if (File.Exists(PLAYER_DATA_FILE_NAME))
            {
                player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
            }
            else
            {
                player = Player.CreateDefaultPlayer();
            }

            MoveTo(player.CurrentLocation);

            UpdatePlayerStats();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToNorth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToWest);
        }

        private void MoveTo(Location newLocation)
        {
            // Необходим ли какой-либо предмет для доступа к локации
            if (!player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                rtbMessages.Text += "Необходимо иметь " + newLocation.ItemRequiredToEnter.Name +
                                    " для входа." + Environment.NewLine;
                ScrollToBottomOfMessages();
                return;
            }

            // Обновить текущее местопложение игрока
            player.CurrentLocation = newLocation;

            // Показать/скрыть доступные кнопки направления движения
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            // Показать наименование и описание текущей локации
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            // Исцелить игрока (происходит каждый раз при смене локации)
            player.CurrentHitPoints = player.MaximumHitPoints;

            // Обновить здоровье в UI
            lblHitPoints.Text = player.CurrentHitPoints.ToString();

            // Доступны ли квесты на данной локации
            if (newLocation.QuestAvailableHere != null)
            {
                // Проверить активен ли какой-либо квест и выполнен ли он
                bool playerAlreadyHasQuest = player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = player.CompletedThisQuest(newLocation.QuestAvailableHere);

                // Квест уже взят
                if (playerAlreadyHasQuest)
                {
                    // Квест еще не выполнен
                    if (!playerAlreadyCompletedQuest)
                    {
                        // Имеются ли предметы для выполнения квеста
                        bool playerHasAllItemsToCompleteQuest = player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                        // Все необходимые предметы есть для выполнения квеста
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            // Показать сообщение
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "Вы выполнили '" + newLocation.QuestAvailableHere.Name + 
                                                "' квест." + Environment.NewLine;
                            ScrollToBottomOfMessages();

                            // Удалить предметы выполненного квеста
                            player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            // Выдать награду за квест
                            rtbMessages.Text += "Вы получили: " + Environment.NewLine;
                            ScrollToBottomOfMessages();
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + 
                                                " очков опыта" + Environment.NewLine;
                            ScrollToBottomOfMessages();
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + 
                                                " золота" + Environment.NewLine;
                            ScrollToBottomOfMessages();
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                            ScrollToBottomOfMessages();
                            rtbMessages.Text += Environment.NewLine;
                            ScrollToBottomOfMessages();

                            player.AddExperiencePoints(newLocation.QuestAvailableHere.RewardExperiencePoints);
                            player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            // Добавить награду в инвентарь игрока
                            player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            // Пометить квест как выполненный
                            player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                        }
                    }
                }
                else
                {
                    // Игрок еще не брал этот квест

                    // Показать сообщение
                    rtbMessages.Text += "Вы получили " + newLocation.QuestAvailableHere.Name + 
                                        " задание." + Environment.NewLine;
                    ScrollToBottomOfMessages();
                    rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                    ScrollToBottomOfMessages();
                    rtbMessages.Text += "Чтобы его выполнить, возвращайтесь с:" + Environment.NewLine;
                    ScrollToBottomOfMessages();
                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine;
                        }
                    }
                    rtbMessages.Text += Environment.NewLine;

                    // Добавить квест в список квестов игрока
                    player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            // Имеется ли монстр на локации?
            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += "Вы видите " + newLocation.MonsterLivingHere.Name + Environment.NewLine;
                ScrollToBottomOfMessages();

                // Создать нового монстра, используя значения из standartMonster в World.Monster list
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                currentMonster = new Monster(standardMonster.ID, standardMonster.Name, 
                                             standardMonster.MaximumDamage, 
                                             standardMonster.RewardExperiencePoints, 
                                             standardMonster.RewardGold, 
                                             standardMonster.CurrentHitPoints, 
                                             standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else
            {
                currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }

            // Обновить статы игрока
            UpdatePlayerStats();

            // Обновить инвентарь
            UpdateInventoryListInUI();

            // Обновить список квестов
            UpdateQuestListInUI();

            // Обновить оружейный комбобокс
            UpdateWeaponListInUI();

            // Обновить комбобокс с целебными зельями
            UpdatePotionListInUI();
        }

        // Обновить статы игрока
        private void UpdatePlayerStats()
        {
            lblHitPoints.Text = player.CurrentHitPoints.ToString();
            lblGold.Text = player.Gold.ToString();
            lblExperience.Text = player.ExperiencePoints.ToString();
            lblLevel.Text = player.Level.ToString();
        }

        // Обновить инвентарь игрока
        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Название";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Количество";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem inventoryItem in player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            }
        }

        // Обновить лист с квестами
        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Название";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Выполнен?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest playerQuest in player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            }
        }

        // Обновить оружейный комбобокс
        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                // Если у игрока отсутствует какое-либо оружие, спрятать кнопку и комбобокс
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                cboWeapons.SelectedIndexChanged -= cboWeapons_SelectedIndexChanged;
                cboWeapons.DataSource = weapons;
                cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;
                cboWeapons.DisplayMember = "Название";
                cboWeapons.ValueMember = "ID";

                if (player.CurrentWeapon != null)
                {
                    cboWeapons.SelectedItem = player.CurrentWeapon;
                }
                else
                {
                    cboWeapons.SelectedIndex = 0;
                }
            }
        }

        // Обновить лечебные зелья игрока
        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem in player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                // У игрока нет исцеляющих снадобий, спрятать кнопку и комбобокс
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Название";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            // Получить текущее оружее из оружейного комбобокса
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            // Определить количество наносимого урона монстру
            int damageToMonster = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, 
                                                                      currentWeapon.MaximumDamage);

            // Нанести урон монстру
            currentMonster.CurrentHitPoints -= damageToMonster;

            // Показать сообщение
            rtbMessages.Text += "Вы нанесли " + currentMonster.Name + " " +
                                damageToMonster.ToString() + " урона." + Environment.NewLine;
            ScrollToBottomOfMessages();

            // Проверить если монстр мертв
            if (currentMonster.CurrentHitPoints <= 0)
            {
                // Монстр мертв
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "Вы победили " + currentMonster.Name + Environment.NewLine;
                ScrollToBottomOfMessages();

                // Выдать очки опыта за победу
                player.AddExperiencePoints(currentMonster.RewardExperiencePoints);
                rtbMessages.Text += "Вы получили " + currentMonster.RewardExperiencePoints.ToString() +
                                    " очка(ов) опыта" + Environment.NewLine;
                ScrollToBottomOfMessages();

                // Выдать золото за победу 
                player.Gold += currentMonster.RewardGold;
                rtbMessages.Text += "Вы получили " + currentMonster.RewardGold.ToString() + 
                                    " золота" + Environment.NewLine;
                ScrollToBottomOfMessages();

                // Выдать случайный лут за победу
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                // Добавить предметы в список lootedItems, сравнив случайное число с процентом дропа
                foreach (LootItem lootItem in currentMonster.LootTable)
                {
                    if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                // Если случайные предметы не были выбраны, добавить предметы по умолчанию
                if (lootedItems.Count == 0)
                {
                    foreach (LootItem lootItem in currentMonster.LootTable)
                    {
                        if (lootItem.IsDefaultItem)
                        {
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                        }
                    }
                }

                // Добавить лут в инвентарь
                foreach (InventoryItem inventoryItem in lootedItems)
                {
                    player.AddItemToInventory(inventoryItem.Details);

                    if (inventoryItem.Quantity == 1)
                    {
                        rtbMessages.Text += "Вы получили " + inventoryItem.Quantity.ToString() + 
                                            " " + inventoryItem.Details.Name + Environment.NewLine;
                        ScrollToBottomOfMessages();
                    }
                    else
                    {
                        rtbMessages.Text += "Вы получили " + inventoryItem.Quantity.ToString() + 
                                            " " + inventoryItem.Details.NamePlural + Environment.NewLine;
                        ScrollToBottomOfMessages();
                    }
                }

                // Обновить информацию об игроке и инвентарь
                lblHitPoints.Text = player.CurrentHitPoints.ToString();
                lblGold.Text = player.Gold.ToString();
                lblExperience.Text = player.ExperiencePoints.ToString();
                lblLevel.Text = player.Level.ToString();

                UpdatePlayerStats();
                UpdateInventoryListInUI();
                UpdateWeaponListInUI();
                UpdatePotionListInUI();

                // Пропустить строку для читаемости
                rtbMessages.Text += Environment.NewLine;

                // Переместить игрока на текущую локацию
                MoveTo(player.CurrentLocation);
            }
            else
            {
                // Монстр все еще жив

                // Определить урон, наносимый игроку
                int damageToPlayer = RandomNumberGenerator.NumberBetween(0, currentMonster.MaximumDamage);

                // Показать сообщение
                rtbMessages.Text += currentMonster.Name + " нанес " + damageToPlayer.ToString() + 
                                    " очков урона." + Environment.NewLine;
                ScrollToBottomOfMessages();

                // Вычесть урон, нанесенный игроку
                player.CurrentHitPoints -= damageToPlayer;

                // Обновить информацию об игроке
                lblHitPoints.Text = player.CurrentHitPoints.ToString();

                if (player.CurrentHitPoints <= 0)
                {
                    // Показать сообщение
                    rtbMessages.Text += currentMonster.Name + " убил вас." + Environment.NewLine;
                    ScrollToBottomOfMessages();

                    // Переместить игрока домой
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // Получить текущее исцеляющее зелье из комбобокса
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            // Добавить количество исцеленного здоровья
            player.CurrentHitPoints = (player.CurrentHitPoints + potion.AmountToHeal);

            // Здоровье не может превышать максимума
            if (player.CurrentHitPoints > player.MaximumHitPoints)
            {
                player.CurrentHitPoints = player.MaximumHitPoints;
            }

            // Удалить использованное исцеляющее зелье из инвенторя
            foreach (InventoryItem ii in player.Inventory)
            {
                if (ii.Details.ID == potion.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }

            // Показать сообщение
            rtbMessages.Text += "Вы выпили " + potion.Name + Environment.NewLine;
            ScrollToBottomOfMessages();

            // Ход монстра

            // Определить наносимый урон игроку
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, currentMonster.MaximumDamage);

            // Показать сообщение
            rtbMessages.Text += currentMonster.Name + " нанес " + damageToPlayer.ToString() + 
                                " очков урона." + Environment.NewLine;
            ScrollToBottomOfMessages();

            // Вычесть нанесенный урон игроку
            player.CurrentHitPoints -= damageToPlayer;

            if (player.CurrentHitPoints <= 0)
            {
                // Показать сообщение
                rtbMessages.Text += currentMonster.Name + " убил вас." + Environment.NewLine;
                ScrollToBottomOfMessages();

                // Перенести игрока домой
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            // Обновить информацию об игроке
            lblHitPoints.Text = player.CurrentHitPoints.ToString();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();
        }

        private void ScrollToBottomOfMessages()
        {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void LittleAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, player.ToXmlString());
        }
        private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }
    }
}
