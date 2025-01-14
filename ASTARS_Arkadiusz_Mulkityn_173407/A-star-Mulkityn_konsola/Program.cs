using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    const int ROZMIAR = 20;
    const int PRZESZKODA = 5;
    const int TRASA = 3;
    const int PUSTE = 0;

    static int[,] mapa = new int[ROZMIAR, ROZMIAR];

    static void Main(string[] args)
    {

        if (!WczytajMape("grid.txt"))
        {
            Console.WriteLine("Blad podczas wczytywania mapy.");
            return;
        }

        List<(int, int)> trasa = ZnajdzTrase((0, 0), (19, 19));

        if (trasa.Count == 0)
        {
            Console.WriteLine("Nie znaleziono trasy.");
        }
        else
        {
            ZaznaczTrase(trasa);
        }

        WyswietlMape();
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
            string[] kolumny = linie[ROZMIAR - 1 - i].Split(' ');

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

        return new List<(int, int)>(); 
    }

    static double Heurystyka((int, int) a, (int, int) b)
    {
        return Math.Sqrt(Math.Pow(a.Item1 - b.Item1, 2) + Math.Pow(a.Item2 - b.Item2, 2)); //pierwiastek z ((poz x - cel x)^2 + (poz y - cel y)^2) 
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

    static void ZaznaczTrase(List<(int, int)> trasa)
    {
        foreach ((int, int) punkt in trasa)
        {
            if (mapa[punkt.Item1, punkt.Item2] != PRZESZKODA)
            {
                mapa[punkt.Item1, punkt.Item2] = TRASA;
            }
        }
    }

    static void WyswietlMape()
    {
        for (int i = ROZMIAR - 1; i >= 0; i--)
        {
            for (int j = 0; j < ROZMIAR; j++)
            {
                Console.Write(mapa[i, j] + " ");
            }
            Console.WriteLine();
        }
    }
}