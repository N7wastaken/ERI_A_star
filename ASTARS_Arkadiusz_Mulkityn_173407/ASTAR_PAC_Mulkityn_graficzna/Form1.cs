using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PacmanAstar
{
    public partial class Form1 : Form
    {
        const int ROZMIAR = 20;
        const int PRZESZKODA = 5;
        const int TRASA = 3;
        const int PUSTE = 0;

        const int TILE_SIZE = 30;
        const int PASEK_WYSOKOSC = 50;

        static int[,] mapa = new int[ROZMIAR, ROZMIAR];
        private List<(int, int)> trasa;

        private Button btnWyznaczTrase;
        private Button btnPrzeszkody;
        private Button btnStartCel;

        private Panel dolnyPasek;
        private bool edytowaniePrzeszkod = false;

        private (int, int)? punktStartowy = null;
        private (int, int)? punktCelowy = null;
        private bool ustawianieStartu = false;
        private bool ustawianieCelu = false;

        public Form1()
        {

            InitializeComponent();

            if (!WczytajMape("grid.txt"))
            {
                MessageBox.Show("Błąd podczas wczytywania mapy.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            this.DoubleBuffered = true;
            trasa = new List<(int, int)>();
        }

        static bool WczytajMape(string sciezkaPliku)
        {
            if (!File.Exists(sciezkaPliku))
            {
                return false;
            }

            string[] linie = File.ReadAllLines(sciezkaPliku);

            if (linie.Length != ROZMIAR)
            {
                return false;
            }

            for (int i = 0; i < ROZMIAR; i++)
            {
                string[] kolumny = linie[i].Split(' ');

                if (kolumny.Length != ROZMIAR)
                {
                    return false;
                }

                for (int j = 0; j < ROZMIAR; j++)
                {
                    if (!int.TryParse(kolumny[j], out mapa[i, j]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        static List<(int, int)> ZnajdzTrase((int, int) start, (int, int) cel)
        {
            List<(int, int)> otwarte = new List<(int, int)>();
            List<(int, int)> zamkniete = new List<(int, int)>();

            (int, int)[,] poprzednik = new (int, int)[ROZMIAR, ROZMIAR];
            double[,] kosztG = new double[ROZMIAR, ROZMIAR];
            double[,] kosztF = new double[ROZMIAR, ROZMIAR];

            for (int i = 0; i < ROZMIAR; i++)
            {
                for (int j = 0; j < ROZMIAR; j++)
                {
                    kosztG[i, j] = double.MaxValue;
                    kosztF[i, j] = double.MaxValue;
                    poprzednik[i, j] = (-1, -1);
                }
            }

            otwarte.Add(start);
            kosztG[start.Item1, start.Item2] = 0;
            kosztF[start.Item1, start.Item2] = Heurystyka(start, cel);

            while (otwarte.Count > 0)
            {
                (int, int) obecny = NajnizszyKosztF(otwarte, kosztF);

                if (obecny == cel)
                {
                    return OdtworzTrase(poprzednik, obecny);
                }

                otwarte.Remove(obecny);
                zamkniete.Add(obecny);

                foreach ((int, int) sasiad in Sasiedzi(obecny))
                {
                    if (zamkniete.Contains(sasiad))
                    {
                        continue;
                    }

                    double nowyKosztG = kosztG[obecny.Item1, obecny.Item2] + 1;

                    if (!otwarte.Contains(sasiad))
                    {
                        otwarte.Add(sasiad);
                    }
                    else if (nowyKosztG >= kosztG[sasiad.Item1, sasiad.Item2])
                    {
                        continue;
                    }

                    poprzednik[sasiad.Item1, sasiad.Item2] = obecny;
                    kosztG[sasiad.Item1, sasiad.Item2] = nowyKosztG;
                    kosztF[sasiad.Item1, sasiad.Item2] = kosztG[sasiad.Item1, sasiad.Item2] + Heurystyka(sasiad, cel);
                }
            }

            return new List<(int, int)>();                                                          // Brak trasy
        }

        static double Heurystyka((int, int) a, (int, int) b)
        {
            return Math.Sqrt(Math.Pow(a.Item1 - b.Item1, 2) + Math.Pow(a.Item2 - b.Item2, 2));      //pierwiastek z ((poz x - cel x)^2 + (poz y - cel y)^2)
        }

        static (int, int) NajnizszyKosztF(List<(int, int)> otwarte, double[,] kosztF)
        {
            (int, int) najnizszy = otwarte[0];
            double najmniejszyKoszt = kosztF[najnizszy.Item1, najnizszy.Item2];

            foreach ((int, int) punkt in otwarte)
            {
                double koszt = kosztF[punkt.Item1, punkt.Item2];
                if (koszt < najmniejszyKoszt)
                {
                    najnizszy = punkt;
                    najmniejszyKoszt = koszt;
                }
            }

            return najnizszy;
        }

        static List<(int, int)> Sasiedzi((int, int) punkt)
        {
            List<(int, int)> sasiedzi = new List<(int, int)>();

            int x = punkt.Item1;
            int y = punkt.Item2;

            if (y < ROZMIAR - 1 && mapa[x, y + 1] != PRZESZKODA) sasiedzi.Add((x, y + 1));
            if (y > 0 && mapa[x, y - 1] != PRZESZKODA) sasiedzi.Add((x, y - 1));
            if (x < ROZMIAR - 1 && mapa[x + 1, y] != PRZESZKODA) sasiedzi.Add((x + 1, y));
            if (x > 0 && mapa[x - 1, y] != PRZESZKODA) sasiedzi.Add((x - 1, y));
            
            return sasiedzi;
        }

        static List<(int, int)> OdtworzTrase((int, int)[,] poprzednik, (int, int) obecny)
        {
            List<(int, int)> trasa = new List<(int, int)>();
            trasa.Add(obecny);

            while (poprzednik[obecny.Item1, obecny.Item2] != (-1, -1))
            {
                obecny = poprzednik[obecny.Item1, obecny.Item2];
                trasa.Add(obecny);
            }

            trasa.Reverse();
            return trasa;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Pen przeszkodaPen = new Pen(Color.Blue, 4);
            Pen przezroczystyPen = new Pen(Color.FromArgb(128, Color.Blue), 1);

            for (int i = 0; i < ROZMIAR; i++)
            {
                for (int j = 0; j < ROZMIAR; j++)
                {
                    Rectangle rect = new Rectangle(j * TILE_SIZE, i * TILE_SIZE, TILE_SIZE, TILE_SIZE);

                    if (mapa[i, j] == PRZESZKODA)
                    {
                        g.FillRectangle(Brushes.Black, rect);
                        g.DrawRectangle(przeszkodaPen, rect);
                    }
                    else if (mapa[i, j] == TRASA)
                    {
                        g.FillRectangle(Brushes.Black, rect);
                        g.FillEllipse(Brushes.Yellow, new Rectangle(
                            rect.X + TILE_SIZE / 4,
                            rect.Y + TILE_SIZE / 4,
                            TILE_SIZE / 2,
                            TILE_SIZE / 2));
                        g.DrawRectangle(przezroczystyPen, rect);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Black, rect);
                        g.DrawRectangle(przezroczystyPen, rect);
                    }
                }
            }

            przeszkodaPen.Dispose();
            przezroczystyPen.Dispose();
        }

        private async Task AnimujTrase(List<(int, int)> trasa)
        {
            foreach ((int, int) punkt in trasa)
            {
                if (mapa[punkt.Item1, punkt.Item2] != PRZESZKODA)
                {
                    mapa[punkt.Item1, punkt.Item2] = TRASA;
                    this.Invalidate();
                    await Task.Delay(100);
                }
            }
        }

        private async void BtnWyznaczTrase_Click(object sender, EventArgs e)
        {
            if (punktStartowy == null || punktCelowy == null)
            {
                MessageBox.Show("Zdefiniuj najpierw punkt startowy i celowy.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Czyszczenie poprzedniej trasy
            foreach ((int, int) punkt in trasa)
            {
                if (mapa[punkt.Item1, punkt.Item2] == TRASA)
                {
                    mapa[punkt.Item1, punkt.Item2] = PUSTE;
                }
            }

            this.Invalidate();                                                                   // Odświeżenie widoku po usunięciu starej trasy

                                                                                                 // Wyznaczanie nowej trasy
            trasa = ZnajdzTrase(punktStartowy.Value, punktCelowy.Value);
            if (trasa.Count == 0)
            {
                MessageBox.Show("Nie znaleziono trasy.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                await AnimujTrase(trasa);
            }
        }

        private void BtnPrzeszkody_Click(object sender, EventArgs e)
        {
            edytowaniePrzeszkod = !edytowaniePrzeszkod;

            if (edytowaniePrzeszkod)
            {
                MessageBox.Show("Tryb edycji przeszkód włączony. Kliknij na kratki, aby dodać lub usunąć przeszkodę.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Tryb edycji przeszkód wyłączony.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            int x = e.Y / TILE_SIZE;
            int y = e.X / TILE_SIZE;

            if (x >= 0 && x < ROZMIAR && y >= 0 && y < ROZMIAR)
            {
                if (ustawianieStartu)
                {
                    if (punktStartowy != null)
                    {
                                                                                                // Usuń poprzednie oznaczenie startu
                        mapa[punktStartowy.Value.Item1, punktStartowy.Value.Item2] = PUSTE;
                    }

                    punktStartowy = (x, y);
                    mapa[x, y] = TRASA;                                                         // Tymczasowe oznaczenie dla widoczności

                    this.Invalidate();                                                          // Odśwież widok
                }
                else if (ustawianieCelu)
                {
                    if (punktCelowy != null)
                    {
                                                                                                // Usuń poprzednie oznaczenie celu
                        mapa[punktCelowy.Value.Item1, punktCelowy.Value.Item2] = PUSTE;
                    }

                    punktCelowy = (x, y);
                    mapa[x, y] = TRASA;                                                         // Tymczasowe oznaczenie dla widoczności

                    this.Invalidate();                                                          // Odśwież widok
                }
                else if (edytowaniePrzeszkod)
                {
                    if (mapa[x, y] == PRZESZKODA)
                    {
                        mapa[x, y] = PUSTE;                                                     // Usuwanie przeszkody
                    }
                    else
                    {
                        mapa[x, y] = PRZESZKODA;                                                // Dodawanie przeszkody
                    }

                    this.Invalidate();                                                          // Odśwież widok
                }
            }
        }

        private void BtnStartCel_Click(object sender, EventArgs e)
        {
            if (!ustawianieStartu && !ustawianieCelu)
            {
                MessageBox.Show("Kliknij na siatkę, aby zdefiniować punkt startowy.\n(aby zdefiniować punkt końcowy kliknij ponownie przycisk)", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ustawianieStartu = true;
            }
            else if (ustawianieStartu)
            {
                MessageBox.Show("Kliknij na siatkę, aby zdefiniować punkt końcowy.\n(aby zatwierdzić punkt końcowy kliknij ponownie przycisk)", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ustawianieStartu = false;
                ustawianieCelu = true;
            }
            else if (ustawianieCelu)
            {
                MessageBox.Show("Punkt startowy i celowy zostały zdefiniowane.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ustawianieCelu = false;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            int szerokoscOkna = ROZMIAR * TILE_SIZE;
            int wysokoscOkna = ROZMIAR * TILE_SIZE + PASEK_WYSOKOSC;

            this.ClientSize = new System.Drawing.Size(szerokoscOkna, wysokoscOkna);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Pacstar Arkadiusz Mulkityn 173407";
            this.Icon = new Icon("Pacman.ico");
            this.StartPosition = FormStartPosition.CenterScreen;



            dolnyPasek = new Panel();
            dolnyPasek.Size = new Size(szerokoscOkna, PASEK_WYSOKOSC);
            dolnyPasek.Location = new Point(0, wysokoscOkna - PASEK_WYSOKOSC);
            dolnyPasek.BackColor = Color.Blue;

            btnWyznaczTrase = new Button();
            btnPrzeszkody = new Button();
            btnStartCel = new Button();

            btnWyznaczTrase.Text = "Wyznacz trasę";
            btnPrzeszkody.Text = "Edytuj przeszkody";
            btnStartCel.Text = "Zdefiniuj start i cel";

            int szerokoscPrzycisku = 180;                                                       // Zwiększono szerokość
            int wysokoscPrzycisku = 35;

            btnWyznaczTrase.Size = btnPrzeszkody.Size = btnStartCel.Size = new Size(szerokoscPrzycisku, wysokoscPrzycisku);

            int odstep = 20;
            int calkowitaSzerokosc = 3 * szerokoscPrzycisku + 2 * odstep;
            int startX = (szerokoscOkna - calkowitaSzerokosc) / 2;
            int startY = (PASEK_WYSOKOSC - wysokoscPrzycisku) / 2;

            btnWyznaczTrase.Location = new Point(startX, startY);
            btnPrzeszkody.Location = new Point(startX + szerokoscPrzycisku + odstep, startY);
            btnStartCel.Location = new Point(startX + 2 * (szerokoscPrzycisku + odstep), startY);

            StylPrzyciskow(btnWyznaczTrase);
            StylPrzyciskow(btnPrzeszkody);
            StylPrzyciskow(btnStartCel);

            btnWyznaczTrase.Click += BtnWyznaczTrase_Click;
            btnPrzeszkody.Click += BtnPrzeszkody_Click;
            btnStartCel.Click += BtnStartCel_Click;

            dolnyPasek.Controls.Add(btnWyznaczTrase);
            dolnyPasek.Controls.Add(btnPrzeszkody);
            dolnyPasek.Controls.Add(btnStartCel);

            this.Controls.Add(dolnyPasek);

            this.ResumeLayout(false);
        }


        private void StylPrzyciskow(Button przycisk)
        {
            przycisk.BackColor = Color.Black;
            przycisk.ForeColor = Color.White;
            przycisk.FlatStyle = FlatStyle.Flat;
            przycisk.FlatAppearance.BorderColor = Color.Gray;
            przycisk.FlatAppearance.BorderSize = 1;
            przycisk.Font = new Font("Arial", 10, FontStyle.Bold);
        }
    }
}