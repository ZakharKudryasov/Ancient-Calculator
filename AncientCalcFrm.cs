using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Ancient_Calculator
{
    public partial class AncientCalcFrm : Form
    {

        Dictionary<string, string[]> rsid = new Dictionary<string, string[]>();
        long total_bp = 0;
        string filename2 = null;
        long total_base_pairs = 0;
        bool m_match_no_call = true;
        long m_AllowedErrors=5;
        long m_BasePairs = 1000000;
        long m_GapToBreak=100000;
        long m_SNPs=150;
        int ancient_index = 0;

        long m_match_error_count=0;
        public AncientCalcFrm()
        {
            InitializeComponent();
        }

        private void DNACalcFrm_Load(object sender, EventArgs e)
        {
            cbAncientList.Items.Add("Ajvide 58");
            cbAncientList.Items.Add("Altai Neanderthal");
            cbAncientList.Items.Add("Australian Aboriginal");
            cbAncientList.Items.Add("BR1");
            cbAncientList.Items.Add("BR2");
            cbAncientList.Items.Add("Clovis Anzick-1");
            cbAncientList.Items.Add("CO1");
            cbAncientList.Items.Add("Denisova");
            cbAncientList.Items.Add("Gökhem2");
            cbAncientList.Items.Add("Hinxton-1");
            cbAncientList.Items.Add("Hinxton-2");
            cbAncientList.Items.Add("Hinxton-3");
            cbAncientList.Items.Add("Hinxton-4");
            cbAncientList.Items.Add("Hinxton-5");
            cbAncientList.Items.Add("IR1");
            cbAncientList.Items.Add("KO1");
            cbAncientList.Items.Add("Kostenki14");
            cbAncientList.Items.Add("La Braña Arintero");
            cbAncientList.Items.Add("Linearbandkeramik");
            cbAncientList.Items.Add("Loschbour");
            cbAncientList.Items.Add("Mal'ta");
            cbAncientList.Items.Add("Motala12");
            cbAncientList.Items.Add("NE1");
            cbAncientList.Items.Add("NE5");
            cbAncientList.Items.Add("NE6");
            cbAncientList.Items.Add("NE7");
            cbAncientList.Items.Add("Palaeo-Eskimo");
            cbAncientList.Items.Add("Ust-Ishim");
            cbAncientList.Items.Add("Vi33.16 Neanderthal");
            cbAncientList.Items.Add("Vi33.25 Neanderthal");
            cbAncientList.Items.Add("Vi33.26 Neanderthal");

            cbAncientList.SelectedIndex = 0;
        }

        public byte[] Zip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public byte[] Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return mso.ToArray();
            }
        }

        public void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                ///
                ancient_index = cbAncientList.SelectedIndex;
                total_bp = 0;
                rsid.Clear();
                neanPercent.Text = "0.00%";
                neanPercent.ForeColor = Color.Gray;
                statusLbl.Text = "Loading ...";
                button1.Enabled = false;

                tbAllowedErrors.Enabled = false;
                tbBasePairs.Enabled = false;
                tbGapToBreak.Enabled = false;
                tbSNPs.Enabled = false;
                cbMatchNoCalls.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;

                long.TryParse(tbAllowedErrors.Text, out m_AllowedErrors);
                long.TryParse(tbBasePairs.Text, out m_BasePairs);
                long.TryParse(tbGapToBreak.Text, out m_GapToBreak);
                long.TryParse(tbSNPs.Text, out  m_SNPs);
                m_match_no_call = cbMatchNoCalls.Checked;
                
                filename2 = dialog.FileName;

                backgroundWorker1.RunWorkerAsync();
            }
        }

        private string getAutosomalText(string file)
        {
            string text = null;

            if (file.EndsWith(".gz"))
            {
                StringReader reader = new StringReader(Encoding.UTF8.GetString(Unzip(File.ReadAllBytes(file))));
                text = reader.ReadToEnd();
                reader.Close();

            }
            else if (file.EndsWith(".zip"))
            {
                using (var fs = new MemoryStream(File.ReadAllBytes(file)))
                using (var zf = new ZipFile(fs))
                {
                    var ze = zf[0];
                    if (ze == null)
                    {
                        throw new ArgumentException("file not found in Zip");
                    }
                    using (var s = zf.GetInputStream(ze))
                    {
                        using (StreamReader sr = new StreamReader(s))
                        {
                            text = sr.ReadToEnd();
                        }
                    }
                }
            }
            else
                text = File.ReadAllText(file);
            return text;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.y-str.org/2014/12/dna-calculator.html");
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            byte[] bytes = null;
            switch (ancient_index)
            {
                case 0: bytes = Ancient_Calculator.Properties.Resources.Ajvide58; break;
                case 1: bytes = Ancient_Calculator.Properties.Resources.Altai_Neanderthal; break;
                case 2: bytes = Ancient_Calculator.Properties.Resources.AusAboriginal; break;
                case 3: bytes = Ancient_Calculator.Properties.Resources.BR1; break;
                case 4: bytes = Ancient_Calculator.Properties.Resources.BR2; break;
                case 5: bytes = Ancient_Calculator.Properties.Resources.Clovis_Anzick_1; break;
                case 6: bytes = Ancient_Calculator.Properties.Resources.CO1; break;
                case 7: bytes = Ancient_Calculator.Properties.Resources.Denisova; break;
                case 8: bytes = Ancient_Calculator.Properties.Resources.Gökhem2; break;
                case 9: bytes = Ancient_Calculator.Properties.Resources.Hinxton_1; break;
                case 10: bytes = Ancient_Calculator.Properties.Resources.Hinxton_2; break;
                case 11: bytes = Ancient_Calculator.Properties.Resources.Hinxton_3; break;
                case 12: bytes = Ancient_Calculator.Properties.Resources.Hinxton_4; break;
                case 13: bytes = Ancient_Calculator.Properties.Resources.Hinxton_5; break;
                case 14: bytes = Ancient_Calculator.Properties.Resources.IR1; break;
                case 15: bytes = Ancient_Calculator.Properties.Resources.KO1; break;
                case 16: bytes = Ancient_Calculator.Properties.Resources.Kostenki14; break;
                case 17: bytes = Ancient_Calculator.Properties.Resources.LaBraña_Arintero; break;
                case 18: bytes = Ancient_Calculator.Properties.Resources.Linearbandkeramik; break;
                case 19: bytes = Ancient_Calculator.Properties.Resources.Loschbour; break;
                case 20: bytes = Ancient_Calculator.Properties.Resources.Mal_ta; break;
                case 21: bytes = Ancient_Calculator.Properties.Resources.Motala12; break;
                case 22: bytes = Ancient_Calculator.Properties.Resources.NE1; break;
                case 23: bytes = Ancient_Calculator.Properties.Resources.NE5; break;
                case 24: bytes = Ancient_Calculator.Properties.Resources.NE6; break;
                case 25: bytes = Ancient_Calculator.Properties.Resources.NE7; break;
                case 26: bytes = Ancient_Calculator.Properties.Resources.Palaeo_Eskimo; break;
                case 27: bytes = Ancient_Calculator.Properties.Resources.Ust_Ishim; break;
                case 28: bytes = Ancient_Calculator.Properties.Resources.Vi33_16_Neanderthal; break;
                case 29: bytes = Ancient_Calculator.Properties.Resources.Vi33_25_Neanderthal; break;
                case 30: bytes = Ancient_Calculator.Properties.Resources.Vi33_26_Neanderthal; break;
                default:
                    bytes = new byte[] { };
                    break;
            }

            StringReader reader = new StringReader(Encoding.UTF8.GetString(Unzip(bytes)));
            string line = null;
            string[] data = null;
            long pos_start = 0;
            long pos_end = 0;
            string chr = null;
            string pchr = null;
            int i_chr = -1;
            total_base_pairs = 0;
            while((line=reader.ReadLine())!=null)
            {
                if (line.StartsWith("#") || line.StartsWith("RSID") || line.Trim().StartsWith("rsid"))
                    continue;
                line = line.Replace("\"", "").Replace("\t", ",");
                data = line.Split(new char[] { ',' });
                if (data.Length == 5)
                    data[3] = data[3] + data[4];

                chr = data[1];

                if (!int.TryParse(chr, out i_chr))
                    continue;
                if (i_chr > 22 || i_chr <= 0)
                    continue;

                if (!rsid.ContainsKey(data[0]))
                    rsid.Add(data[0], data);

                if (chr != pchr || long.Parse(data[2]) - pos_end >= m_GapToBreak)
                {

                    total_base_pairs = total_base_pairs + (pos_end - pos_start);
                    pos_start = long.Parse(data[2]);
                }
                pos_end = long.Parse(data[2]);
                pchr = chr;
            }
            reader.Close();
            //
            if(pchr==chr)
                total_base_pairs = total_base_pairs + (pos_end - pos_start);            
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            total_bp = 0;
            neanPercent.Text = "0.00%";
            neanPercent.ForeColor = Color.Gray;
            statusLbl.Text = "Calculating ...";
            backgroundWorker2.RunWorkerAsync();

        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            StringReader reader = new StringReader(getAutosomalText(filename2));
            string line = null;
            string[] data = null;            
            long segment_start = 0;
            long segment_end = 0;
            string chr=null;
            string pchr=null;
            int i_chr=-1;
            int snp_count = 0;
            bool doub = true;
            m_match_error_count = 0;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#") || line.StartsWith("RSID"))
                    continue;
                line = line.Replace("\"", "").Replace("\t", ",");
                data = line.Split(new char[] { ',' });
                if (data.Length == 5)
                    data[3] = data[3] + data[4];
                if(rsid.ContainsKey(data[0]))
                {
                    chr=data[1];

                    if (!isDoubleMatch(rsid[data[0]][3], data[3]))
                        doub = false;

                    if (!int.TryParse(chr, out i_chr))
                        continue;
                    if (i_chr > 22 || i_chr <= 0)
                        continue;      
                    else if (!isMatch(rsid[data[0]][3], data[3]) || chr != pchr)
                    {
                        if (segment_end - segment_start >= m_BasePairs && snp_count > m_SNPs) // 100000 bp
                        {
                            if (doub)
                                total_bp = total_bp + (segment_end - segment_start)*2;
                            else
                                total_bp = total_bp + (segment_end - segment_start);                            
                        }
                        doub = true;
                        segment_start = long.Parse(rsid[data[0]][2]);
                        snp_count = 0;
                        m_match_error_count = 0;
                    }
                    segment_end = long.Parse(rsid[data[0]][2]);
                    pchr=chr;
                    snp_count++;
                }
            }
            reader.Close();
        }

        public static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        private bool isDoubleMatch(string p1, string p2)
        {
            if (p1 == p2 || p1==ReverseString(p2))
                 return true;
            return false;
        }

        private bool isMatch(string p1, string p2)
        {
            foreach (char c1 in p1.ToCharArray())
                foreach (char c2 in p2.ToCharArray())
                    if (c1 == c2)
                        return true;
                    else if (m_match_no_call && (c1 == '-' || c2 == '-' || c1 == '?' || c2 == '?' || c1 == '0' || c2 == '0'))
                        return true;

            m_match_error_count++;
            if (m_match_error_count <= m_AllowedErrors)
                return true;
            return false;
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            // because total_bp is HIR, we need to calcualate percentage for each allele.
            double percent = total_bp * 100.0 / (total_base_pairs * 2);// excluding X chromosome and positions not tested by DNA companies and mtdna otherwise it is 3.2 billion bp
            if (percent > 100)
                percent = 100;
            neanPercent.Text = percent.ToString("#0.00") + "%";
            neanPercent.ForeColor = Color.Black;
            statusLbl.Text = "";
            button1.Enabled = true;
            //
            tbAllowedErrors.Enabled = true;
            tbBasePairs.Enabled = true;
            tbGapToBreak.Enabled = true;
            tbSNPs.Enabled = true;
            cbMatchNoCalls.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tbAllowedErrors.Text = "1";
            tbBasePairs.Text =  "100000";
            tbGapToBreak.Text = "100000";
            tbSNPs.Text = "60";
            cbMatchNoCalls.Checked = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tbAllowedErrors.Text = "5";
            tbBasePairs.Text = "1000000";
            tbGapToBreak.Text = "100000";
            tbSNPs.Text = "150";
            cbMatchNoCalls.Checked = true;
        }
    }
}
