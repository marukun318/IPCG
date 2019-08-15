/// 超雑に描いたIchigojam PCGコンバータ
/// By marukun

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace IJPCG
{
    public partial class Form1 : Form
    {

        public int lno = 100;                   // 行番号
        public int adr = 0x700;                 // POKEするアドレス

        public Form1()
        {
            InitializeComponent();
            printStat("Ready.");
        }

        /// <summary>
        /// 開くメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            ofd.FileName = "";
            //はじめに表示されるフォルダを指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            ofd.InitialDirectory = "";
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter = "PNGファイル(*.png)|*.png|すべてのファイル(*.*)|*.*";
            //[ファイルの種類]ではじめに選択されるものを指定する
            //2番目の「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 1;
            //タイトルを設定する
            ofd.Title = "開くファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;
            //存在しないファイルの名前が指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckFileExists = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckPathExists = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            printStat("開く " + ofd.FileName);

            // PCG変換
            ConvertBitmap(ofd.FileName);

        }

        /// <summary>
        /// 終了メニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        #region 汎用メソッド


        /// <summary>
        /// ビットマップ変換
        /// </summary>
        /// <param name="filename"></param>
        private void ConvertBitmap(string filename)
        {
//            print("ConvertBitmap(" + filename + ")");

            Bitmap bitmap = new Bitmap(filename);

 //           print("Pixel format: " + bitmap.PixelFormat.ToString());
 //           print("Width: " + bitmap.Width + " Height: " + bitmap.Height);

            for (int j=0; j< bitmap.Height; j += 8)
            {
                for (int i = 0; i < bitmap.Width; i += 8)
                {
                    var sb = new StringBuilder();
                    sb.Append(lno.ToString());
                    sb.AppendFormat(" POKE #{0:X3},",adr);
                    for (int y = 0; y < 8; y++)
                    {
                        int bt = 0;
                        int py = j + y;
                        for (int x = 0; x < 8; x++)
                        {
                            int px = i + x;
                            Color col = bitmap.GetPixel(px, py);
                            bt <<= 1;
                            if (col.A > 128)
                            {
                                bt |= 1;
                            }
                        }
                        sb.AppendFormat(bt.ToString());
                        if (y < 7)
                        {
                            sb.Append(',');
                        } else
                        {
                            print(sb.ToString());
                            lno += 10;
                        }
                        adr++;
                    }

                }

            }
        }


        /// <summary>
        /// 文字列表示
        /// </summary>
        /// <param name="str"></param>
        public void print(String str)
        {
            // TextBoxのプロパティ設定
            // AcceptReturn True
            // HideSelection false
            // MultiLine true
            //
            textBox.Select(textBox.TextLength, 0);
            StringBuilder sb = new StringBuilder(str, 1024);
            sb.AppendLine();

            textBox.AppendText(sb.ToString());

            Update();
            Application.DoEvents();     // メッセージループ

            sb = null;
        }
        /// <summary>
        /// デバッグ用print(フォーマット対応）
        /// </summary>
        /// <param name="fmt">フォーマット文字列</param>
        /// <param name="arg">引数</param>
        public void print(string fmt, params object[] arg)
        {
            int len = arg.Length;

            if (len <= 0)
            {
                print(fmt);
            }
            else
            {
                print(String.Format(fmt, arg));
            }
        }

        /// <summary>
        /// ステータスバーに文字列表示
        /// </summary>
        /// <param name="str">表示文字列</param>
        public void printStat(String str)
        {
            //            ToolStripStatusLabel1.Text = str;
            toolStripStatusLabel.Text = str;

        }

        /// <summary>
        /// 文字列を10進数(int)に変換
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns>数値</returns>
        public int atoi(String str)
        {
            int r = 0;

            try
            {
                r = Int32.Parse(str);
            }
            catch (Exception e)
            {
#if DEBUG
                print(e.Message);
#endif
                printStat(String.Format("文字列の書式が間違っています '{0}'", str));
            }

            return r;
        }

        #endregion


        #region イベントハンドラ

        /// <summary>
        /// TextBoxのDragDropイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_DragDrop(object sender, DragEventArgs e)
        {
            //コントロール内にドロップされたとき実行される
            //ドロップされたすべてのファイル名を取得する
            string[] fileName =
                (string[])e.Data.GetData(DataFormats.FileDrop, false);
            //ListBoxに追加する
            printStat("D&D " + fileName[0]);

            foreach (var fname in fileName)
            {
                // Bitmap変換
                ConvertBitmap(fname);
            }
        }

        /// <summary>
        /// TextBoxのDragEnterイベントハンドラ 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_DragEnter(object sender, DragEventArgs e)
        {
            //コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                //ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            else
                //ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// Formのロード時の初期化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            linetextBox.Text = lno.ToString();
        }

        /// <summary>
        /// 行番号変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinetextBox_TextChanged(object sender, EventArgs e)
        {
            lno = atoi(linetextBox.Text);
        }
    }

    #endregion
}
