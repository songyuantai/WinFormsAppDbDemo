using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace WinFormsAppDbDemo
{
    public partial class Form1 : Form
    {
        private const string CONNECT_STR = "Server=127.0.0.1;Database=costdb;User Id=root;Password=root;Port=3306;";
        private static readonly DatabaseHelper _db = new(CONNECT_STR, DatabaseProvider.MySql);
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            this.BeginInvoke(new Action(() =>
            {
                var date = dateTimePicker1.Value;
                var sql = $" select * from serial_nos where year_num = @year_num and month_num = @month_num order by year_num, month_num, seq_no";
                var parameters = new DbParameter[]
                {
                    _db.CreateDbParameter("year_num", date.Year)!,
                    _db.CreateDbParameter("month_num", date.Month)!,
                };

                var dt = _db.ExecuteDataTable(sql, CommandType.Text, parameters);
                var list = new List<SerialNos>();
                foreach (DataRow row in dt.Rows)
                {
                    var item = new SerialNos()
                    {
                        Id = Convert.ToInt32(row["ID"]),
                        Year = Convert.ToInt32(row["YEAR_NUM"]),
                        Month = Convert.ToInt32(row["MONTH_NUM"]),
                        Sequnce = Convert.ToInt32(row["SEQ_NO"]),
                        SerialNo = row["SERIAL_NO"].ToString(),
                    };

                    list.Add(item);
                }

                listView1.View = View.List;
                listView1.Items.Clear();
                foreach (var item in list)
                {
                    var view = new ListViewItem
                    {
                        Text = item.SerialNo,
                        Tag = item.Id
                    };

                    listView1.Items.Add(view);
                }
            }));
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var date = dateTimePicker1.Value;
            var seqNo = GetSeqNo(date.Year, date.Month);
            var no = new SerialNos()
            {
                Year = date.Year,
                Month = date.Month,
                SerialNo = date.ToString("yyyyMM") + seqNo.ToString().PadLeft(2, '0'),
                Sequnce = seqNo
            };

            int tryout = 0;
            while (true)
            {
                if (!AddNo(no, out bool redo))
                {
                    //最多尝试5次
                    if (redo && tryout <= 5)
                    {
                        tryout++;
                        continue;
                    }

                    MessageBox.Show("添加失败，请检查！");
                    return;
                }
                else
                {
                    MessageBox.Show("添加成功！");
                    LoadData();
                    return;
                }
            }


        }

        private bool AddNo(SerialNos no, out bool redo)
        {
            redo = false;
            try
            {
                var sql = @"insert into serial_nos(year_num, month_num, serial_no, seq_no) values
                (@year_num, @month_num, @serial_no, @seq_no)";

                var parameters = new DbParameter[]
                {
                    _db.CreateDbParameter("year_num", no.Year ?? 0)!,
                    _db.CreateDbParameter("month_num", no.Month ?? 0)!,
                    _db.CreateDbParameter("serial_no", no.SerialNo ?? string.Empty)!,
                    _db.CreateDbParameter("seq_no", no.Sequnce ?? 0)!,
                };

                var num = _db.ExecuteNonQuery(sql, CommandType.Text, parameters);

                return num > 0;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2627)
                {
                    redo = true;

                }
                return false;
            }
            catch (Exception)
            {

                return false;
            }

        }

        private int GetMaxSeqNo(int year, int month)
        {
            var sql = "select max(seq_no) from serial_nos where year_num = @year_num and month_num = @month_num";
            var parameters = new DbParameter[]
            {
                _db.CreateDbParameter("year_num", year)!,
                _db.CreateDbParameter("month_num", month)!,
            };

            var value = _db.ExecuteScalar(sql, CommandType.Text, parameters);
            if (int.TryParse(value.ToString(), out int num))
            {
                return num + 1;
            }

            return 1;
        }

        private int GetSeqNo(int year, int month)
        {
            var sql = @"select min(a.seq_no) from serial_nos a
                left join serial_nos b on a.seq_no + 1 = b.seq_no
                where b.id is null and a.year_num = @year_num and a.month_num = @month_num";

            var parameters = new DbParameter[]
            {
                _db.CreateDbParameter("year_num", year)!,
                _db.CreateDbParameter("month_num", month)!,
            };

            var value = _db.ExecuteScalar(sql, CommandType.Text, parameters);
            if (int.TryParse(value.ToString(), out int num))
            {
                return num + 1;
            }

            return 1;
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            var idList = new List<int>();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                idList.Add(Convert.ToInt32(item.Tag));
            }

            if (idList.Count == 0)
            {
                MessageBox.Show("请选择删除的项目");
                return;
            }

            foreach (var id in idList)
            {
                Delete(id);
            }

            MessageBox.Show("删除成功！");
            LoadData();
        }

        private void Delete(int id)
        {
            var sqlLock = "select * from serial_nos where id = @id for update";
            var sqlDel = $"delete from serial_nos where id = @id";
            _db.ExecuteInTransaction((db, con, tran) =>
            {
                //行锁
                db.ExecuteNonQuery(con, tran, sqlLock, CommandType.Text, db.CreateDbParameter("id", id)!);

                //删除
                db.ExecuteNonQuery(con, tran, sqlDel, CommandType.Text, db.CreateDbParameter("id", id)!);

                //释放
            });

        }
    }
}
