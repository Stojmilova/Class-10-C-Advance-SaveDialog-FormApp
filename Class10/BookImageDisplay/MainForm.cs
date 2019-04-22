using BooksProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BookImageDisplay
{
    public partial class MainForm : Form
    {
        public IEnumerable<Author> authors;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // authors are gotten from http://www.worldswithoutend.com
            BooksLoader booksLoader = new BooksLoader();
            authors = booksLoader.GetAllAuthors();
            lbxAuthors.DataSource = authors;
            lbxAuthors.DisplayMember = "Name";
        }
        private void lbxAuthors_SelectedIndexChanged(object sender, EventArgs e)
        {
            var author = lbxAuthors.SelectedItem as Author;
            lbxBooks.DataSource = author.Books;
            lbxBooks.DisplayMember = "Title";
        }
        private void lbxBooks_SelectedIndexChanged(object sender, EventArgs e)
        {
            var author = lbxAuthors.SelectedItem as Author;
            var book = lbxBooks.SelectedItem as Book;
            var bookName = string.Join("", book.Title.Split(' '));
            bookName = Regex.Replace(bookName, "[']", "");
            bookName = bookName.Substring(0, Math.Min(8, bookName.Length));

            //********If there books with more than one author:

            var authorInitials = "";
            var imageName = "";

            var booksWithMoreThanOneAuthor = authors.SelectMany(a => a.Books)
                     .GroupBy(b => b.Title)
                     .Where(g => g.Count() > 1)
                     .Select(g => new { BookTitle = g.Key });

            var booksWithMoreThanOneAuthorCount = booksWithMoreThanOneAuthor.Count(b => b.BookTitle == book.Title);

            var authorName = authors.Select(a => new
            {
                a.Name,
                BooksTitles = a.Books.Select(b => b.Title)

            }).Where(a => a.BooksTitles.Contains(book.Title))
              .Select(a => new { a.Name });


            if (booksWithMoreThanOneAuthorCount > 1)
            {
                foreach (var name in authorName)
                {
                    var separator = Path.DirectorySeparatorChar;
                    authorInitials += string.Join("", name.Name.Split(' ').Select(p => p[0])).ToString() + separator;
                }
                imageName = $"http://www.worldswithoutend.com/covers_ml/{authorInitials}_{bookName}.jpg";
            }
            else
            {
                foreach (var item in booksWithMoreThanOneAuthor)
                {
                    authorInitials = string.Join("", author.Name.Split(' ').Select(p => p[0])).ToString();
                    imageName = $"http://www.worldswithoutend.com/covers_ml/{authorInitials}_{bookName}.jpg";
                }             
            }

            try
            {
                pbxCover.Load(imageName);
            }
            catch (WebException wex)
            {
                var response = wex.Response as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    pbxCover.Load("https://via.placeholder.com/235x375.png?text=No+image+found");
                }
                else
                {
                    throw;
                }
            }
        }
        private void lnkNovelPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var book = lbxBooks.SelectedItem as Book;
            var url = $"http://www.worldswithoutend.com/novel.asp?ID={book.ID}";
            ProcessStartInfo sInfo = new ProcessStartInfo(url);
            Process.Start(sInfo);
        }
        private void buttonToSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog savefile = new SaveFileDialog();            
            savefile.Filter= "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png|All files (*.*)|*.*";
            if (DialogResult.OK == savefile.ShowDialog())
            {
                string extension = System.IO.Path.GetExtension(savefile.FileName);

                if(extension == ".jpg")
                {
                    pbxCover.Image.Save(savefile.FileName, ImageFormat.Jpeg);
                   
                }else if(extension == ".bmp")
                {
                    pbxCover.Image.Save(savefile.FileName, ImageFormat.Bmp);                  
                }
                else if(extension == ".gif")
                {
                    pbxCover.Image.Save(savefile.FileName, ImageFormat.Gif);
                   
                }else if(extension == ".png")
                {
                    pbxCover.Image.Save(savefile.FileName, ImageFormat.Png);
                    
                }else
                {
                    throw new ArgumentOutOfRangeException(extension);
                }                      
            }
        }   
    }
}
