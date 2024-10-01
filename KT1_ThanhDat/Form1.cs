using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KT1_ThanhDat
{
    public partial class Form1 : Form
    {
        string strcon = @"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=..\..\..\DATA\QLSV.mdb";
        DataSet ds = new DataSet();
        //
        OleDbDataAdapter adpSinhVien, adpMonHoc, adpKetQua;
        //
        OleDbCommandBuilder cmdSinhVien;
        BindingSource bs = new BindingSource();
        public Form1()
        {
            InitializeComponent();
            bs.CurrentChanged += Bs_CurrentChanged;
        }

        private void Bs_CurrentChanged(object sender, EventArgs e)
        {
            
            lblstt.Text = bs.Position + 1 + "/" + bs.Count;
            string mmh = txtMamh.Text;
            DemTSSV(txtMamh.Text);
            txtDiemlonnhat.Text= LayDiemLonNhat(mmh).ToString();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            KhoiTaoCacDoiTuong();
            DocDuLieu();
            MocNoiQuanHe();
            KhoiTaoBindingSoure();
            LienKetDieuKhien();

            bdnmonhoc.BindingSource = bs;


        }
        private int LayDiemLonNhat(string mmh)
        {
            if (string.IsNullOrEmpty(mmh))
            {
                return 0; 
            }
            object diemLonNhat = ds.Tables["KETQUA"].Compute("MAX(Diem)", "MaMH='" + mmh + "'");

            return diemLonNhat != DBNull.Value ? Convert.ToInt32(diemLonNhat) : 0;
        }
        private void DemTSSV(string mmh)
        {
            if (string.IsNullOrEmpty(mmh))
            {
                txtTSSV.Text = "0";
                return;
            }

            int tssv = (int)ds.Tables["KETQUA"].Compute("COUNT(MaSV)", "MaMH='" + mmh + "'");

            txtTSSV.Text = tssv.ToString();

            var sinhViens = ds.Tables["KETQUA"].AsEnumerable()
                .Where(row => row.Field<string>("MaMH") == mmh)
                .Select(row => row.Field<string>("MaSV"))
                .Distinct();

            string danhSachSinhVien = string.Join(", ", sinhViens);
            txtTSSV.Text += "";
        }
        private void LienKetDieuKhien()
            {
                foreach (Control ctl in this.Controls)
                    if (ctl is TextBox && ctl.Name != "txtDiemlonnhat" && ctl.Name != "txtTSSV" && ctl.Name != "txtLoaimon")
                        ctl.DataBindings.Add("text", bs, ctl.Name.Substring(3), true);

                Binding bdLoaimh = new Binding("text", bs, "Loaimh", true);

                bdLoaimh.Format += BdLoaimh_format;
                bdLoaimh.Parse += BdLoaimh_Parse;
                txtLoaimon.DataBindings.Add(bdLoaimh);
            }

        private void BdLoaimh_format(object sender, ConvertEventArgs e)
        {
            if (e.Value == DBNull.Value || e.Value == null) return;
            e.Value = (Boolean)e.Value ? "Bat buoc" : "Tuy Chon";
        }

        private void BdLoaimh_Parse(object sender, ConvertEventArgs e)
        {
            if (e.Value == null) return;
            e.Value = e.Value.ToString().ToUpper() == "BAT BUOT" ? true : false;
        }

        private void KhoiTaoBindingSoure()
        {

            bs.DataSource = ds;
            bs.DataMember = "MONHOC";
        }

        private void MocNoiQuanHe()
        {
            ds.Relations.Add("FK_MH_KQ", ds.Tables["MONHOC"].Columns["MaMH"], ds.Tables["KETQUA"].Columns["MaMH"], true);

            ds.Relations.Add("FK_SV_KQ", ds.Tables["SINHVIEN"].Columns["MaSV"], ds.Tables["KETQUA"].Columns["MaSV"], true);

            ds.Relations["FK_MH_KQ"].ChildKeyConstraint.DeleteRule = Rule.None;
            ds.Relations["FK_SV_KQ"].ChildKeyConstraint.DeleteRule = Rule.None;
        }

        private void DocDuLieu()
        {
            adpMonHoc.FillSchema(ds, SchemaType.Source, "KHOA");
            adpMonHoc.Fill(ds, "MONHOC");

            adpSinhVien.FillSchema(ds, SchemaType.Source, "SINHVIEN");
            adpSinhVien.Fill(ds, "SINHVIEN");

            adpKetQua.FillSchema(ds, SchemaType.Source, "KETQUA");
            adpKetQua.Fill(ds, "KETQUA");
        }

        private void btThem_Click(object sender, EventArgs e)
        {

            txtMamh.ReadOnly = false;

            bs.AddNew();
            
            
            txtMamh.Focus();
        }

        private void txtDiemlonnhat_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void btTruot_Click(object sender, EventArgs e)
        {
            if (bs.Count > 0)
            {
                // Kiểm tra nếu không phải môn học cuối
                if (bs.Position < bs.Count - 1)
                {
                    bs.MoveNext(); // Di chuyển tới môn học tiếp theo
                }
                else
                {
                    MessageBox.Show("Đã ở môn học cuối cùng!"); // Thông báo nếu đã đến cuối
                }
            }
        }

        private void btSau_Click(object sender, EventArgs e)
        {
            if (bs.Count > 0)
            {
                if (bs.Position > 0)
                {
                    bs.MovePrevious();
                }
                else
                {
                    MessageBox.Show("Đã ở môn học đầu tiên!"); // Thông báo nếu đã đến đầu
                }
            }
        }

        private void btDau_Click(object sender, EventArgs e)
        {
            if (bs.Count > 0)
            {
                bs.MoveFirst(); 
            }
        }

        private void btCuoi_Click(object sender, EventArgs e)
        {
            if (bs.Count > 0)
            {
                bs.MoveLast();
            }
        }

        private void btThoat_Click(object sender, EventArgs e)
        {

            DialogResult result = MessageBox.Show("Bạn chắc chắn muốn đóng cửa sổ này?","Xác nhận đóng cửa sổ", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                this.Close();
            }
           
        }

        private void btGhi_Click(object sender, EventArgs e)
        {
            try
            {
                
                bs.EndEdit(); 
                int rowsAffected = adpSinhVien.Update(ds, "SINHVIEN");
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Ghi dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Không có thay đổi nào được ghi lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                MessageBox.Show("Đã xảy ra lỗi khi ghi dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btHuy_Click(object sender, EventArgs e)
        {
            bs.CancelEdit();
        }

        private void KhoiTaoCacDoiTuong()
        {
            adpMonHoc = new OleDbDataAdapter("select * from MonHoc ", strcon);
            adpSinhVien = new OleDbDataAdapter("select * from SinhVien ", strcon);
            adpKetQua = new OleDbDataAdapter("select * from KetQua ", strcon);

            cmdSinhVien = new OleDbCommandBuilder(adpSinhVien);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
