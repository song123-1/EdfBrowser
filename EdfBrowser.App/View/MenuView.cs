using EdfBrowser.App.ViewModels;
using System;
using System.Linq;
using System.Windows.Forms;

namespace EdfBrowser.App.View
{
    public partial class MenuView : UserControl
    {

        private readonly MenuViewModel m_menuViewModel;

        public MenuView(MenuViewModel menuViewModel)
        {
            InitializeComponent();

            m_menuViewModel = menuViewModel;

            BackColor = System.Drawing.Color.White;

            Load += MenuView_Load;
        }

        private void MenuView_Load(object sender, System.EventArgs e)
        {
            // Create Menu Controls
            MenuStrip menuStrip = new MenuStrip();
            menuStrip.Dock = DockStyle.Fill;

            // Add Menu Items
            m_menuViewModel.LoadMenuCommand.Execute(null);
            if (m_menuViewModel.Menus.Count() == 0) return;

            foreach (var menu in m_menuViewModel.Menus)
            {
                ToolStripMenuItem rootMenuItem = new ToolStripMenuItem(menu.Description);
                if (menu.MenuItems != null)
                {
                    foreach (var item in menu.MenuItems)
                    {
                        ToolStripMenuItem subMenuItem = new ToolStripMenuItem(item.Description);
                        subMenuItem.Click += OnSubMenuItemClick;
                        rootMenuItem.DropDownItems.Add(subMenuItem);
                    }
                }

                menuStrip.Items.Add(rootMenuItem);
            }

            // Add the Menu Controls to the UserControl
            Controls.Add(menuStrip);
        }

        private void OnSubMenuItemClick(object sender, EventArgs e)
        {
            var clickedItem = sender as ToolStripMenuItem;
            if (clickedItem == null) return;

            var strategy = StrategyFactory.GetStrategy(clickedItem.Text);
            if (strategy != null)
            {
                if (strategy is OpenStrategy openStrategy)
                {
                    openStrategy.FileSelected += OnFileSelected;
                }
            }

            strategy?.Execute(); // 如果命令存在则执行
        }

        private void OnFileSelected(object sender, string path)
        {
            EdfDashBoardView edfDashBoardView = new EdfDashBoardView();
            Form form = new Form();
            form.MinimumSize = new System.Drawing.Size(600, 600);
            form.Text = "Add Signal";

            form.Controls.Add(edfDashBoardView);

            edfDashBoardView.Dock = DockStyle.Fill;
            edfDashBoardView.Show();
            form.ShowDialog();
        }
    }

    public interface IStrategy
    {
        void Execute();
    }

    public class OpenStrategy : IStrategy
    {
        public event EventHandler<string> FileSelected;

        public void Execute()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "EDF files (*.edf)|*.edf";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FileSelected?.Invoke(this, openFileDialog.FileName);
                }
            }
        }
    }

    public class CloseStrategy : IStrategy
    {
        public void Execute()
        {

        }
    }

    public static class StrategyFactory
    {
        public static IStrategy GetStrategy(string description)
        {
            switch (description)
            {
                case "Open":
                    return new OpenStrategy();
                case "Close":
                    return new CloseStrategy();
                // 可以添加更多的选项
                default:
                    return null; // 默认情况
            }
        }
    }
}
