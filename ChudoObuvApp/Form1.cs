using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ChudoObuvApp
{
    public partial class Form1 : Form
    {
        // Строка подключения к локальной БД ChudoObuv
        private readonly string connectionString =
            @"Data Source=DESKTOP-ATB9EHB\SQLEXPRESS;
              Initial Catalog=ChudoObuv;
              Integrated Security=True;
              TrustServerCertificate=True;";

        private string currentTable = "";
        private SqlDataAdapter adapter;
        private DataTable dataTable;

        public Form1()
        {
            InitializeComponent();
            LoadTableList();
        }

        /// <summary>
        /// Загружает список пользовательских таблиц из БД в ComboBox
        /// </summary>
        private void LoadTableList()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT TABLE_NAME 
                                   FROM INFORMATION_SCHEMA.TABLES 
                                   WHERE TABLE_TYPE = 'BASE TABLE' 
                                   ORDER BY TABLE_NAME";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        cmbTables.Items.Clear();
                        while (reader.Read())
                            cmbTables.Items.Add(reader.GetString(0));
                    }
                }
                if (cmbTables.Items.Count > 0)
                    cmbTables.SelectedIndex = 0;

                lblStatus.Text = "Список таблиц загружен.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки списка таблиц:\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Загружает данные выбранной таблицы в DataGridView
        /// </summary>
        private void LoadTableData()
        {
            if (string.IsNullOrEmpty(currentTable)) return;

            try
            {
                dataTable = new DataTable();
                adapter = new SqlDataAdapter($"SELECT * FROM [{currentTable}]", connectionString);

                // Автогенерация команд INSERT, UPDATE, DELETE
                new SqlCommandBuilder(adapter);

                adapter.Fill(dataTable);
                dgvData.DataSource = dataTable;

                lblStatus.Text = $"Загружено строк: {dataTable.Rows.Count} (таблица {currentTable})";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных:\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Кнопка «Загрузить»
        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (cmbTables.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу из списка.",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            currentTable = cmbTables.SelectedItem.ToString();
            LoadTableData();
        }

        // Кнопка «Добавить» — новая пустая строка
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (dataTable == null)
            {
                MessageBox.Show("Сначала загрузите таблицу.",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            dataTable.Rows.Add(dataTable.NewRow());
        }

        // Кнопка «Изменить» — сохранить изменения в БД
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (adapter == null || dataTable == null)
            {
                MessageBox.Show("Сначала загрузите таблицу.",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                dgvData.EndEdit();
                adapter.Update(dataTable);
                lblStatus.Text = "Изменения сохранены.";
                LoadTableData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения изменений:\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Кнопка «Удалить» — удалить выбранную строку
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvData.CurrentRow == null || dataTable == null)
            {
                MessageBox.Show("Нет выбранной строки для удаления.",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить выбранную строку?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                dgvData.Rows.Remove(dgvData.CurrentRow);
                adapter.Update(dataTable);
                lblStatus.Text = "Строка удалена.";
            }
        }
    }
}