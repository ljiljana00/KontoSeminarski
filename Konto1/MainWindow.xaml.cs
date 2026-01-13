using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private string connString = "server=localhost;database=ris;uid=root;pwd=;";
        private int trenutniId = -1;

        public MainWindow()
        {
            InitializeComponent();
            UcitajPodatke();
        }

        private void UcitajPodatke(string pretraga = "")
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    string sql = "SELECT * FROM konto";
                    if (!string.IsNullOrEmpty(pretraga))
                    {
                        sql += $" WHERE konto LIKE '{pretraga}%' OR naziv LIKE '%{pretraga}%'";
                    }

                    MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgKonto.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { MessageBox.Show("Greška: " + ex.Message); }
        }

        private void BtnUnesi_Click(object sender, RoutedEventArgs e)
        {
            // VALIDACIJA: Provera da li su polja prazna
            if (string.IsNullOrWhiteSpace(txtKonto.Text) || string.IsNullOrWhiteSpace(txtNaziv.Text))
            {
                MessageBox.Show("Morate uneti i Šifru konta i Naziv!", "Obavezan unos", MessageBoxButton.OK, MessageBoxImage.Warning);

                if (string.IsNullOrWhiteSpace(txtKonto.Text)) txtKonto.Focus();
                else txtNaziv.Focus();

                return;
            }

            IzvrsiUpit($"INSERT INTO konto (konto, naziv, prenos) VALUES ('{txtKonto.Text}', '{txtNaziv.Text}', '{txtPrenos.Text}')", "Podaci su uspešno sačuvani.");
            OcistiSve();
        }

        private void BtnIzmeni_Click(object sender, RoutedEventArgs e)
        {
            if (trenutniId != -1)
            {
                // VALIDACIJA: Provera i pri izmeni
                if (string.IsNullOrWhiteSpace(txtKonto.Text) || string.IsNullOrWhiteSpace(txtNaziv.Text))
                {
                    MessageBox.Show("Polja Šifra i Naziv ne smeju biti prazna prilikom izmene!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                IzvrsiUpit($"UPDATE konto SET konto='{txtKonto.Text}', naziv='{txtNaziv.Text}', prenos='{txtPrenos.Text}' WHERE id={trenutniId}", "Izmene su sačuvane.");
                OcistiSve();
            }
            else { MessageBox.Show("Prvo kliknite na red u tabeli koji želite menjati."); }
        }

        private void BtnObrisi_Click(object sender, RoutedEventArgs e)
        {
            if (trenutniId != -1)
            {
                if (MessageBox.Show("Brišete izabrani konto. Da li ste sigurni?", "Potvrda brisanja", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    IzvrsiUpit($"DELETE FROM konto WHERE id={trenutniId}", "Konto je obrisan.");
                    OcistiSve();
                }
            }
        }

        // DUGME OSVEŽI / OČISTI
        private void BtnOsvezi_Click(object sender, RoutedEventArgs e)
        {
            OcistiSve(); // Briše tekst iz polja
            UcitajPodatke(); // Ponovo učitava tabelu
            txtPretraga.Clear(); // Briše pretragu
        }

        // DUGME IZLAZ
        private void BtnIzlaz_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TxtPretraga_TextChanged(object sender, TextChangedEventArgs e)
        {
            UcitajPodatke(txtPretraga.Text);
        }

        private void DgKonto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgKonto.SelectedItem is DataRowView red)
            {
                trenutniId = Convert.ToInt32(red["id"]);
                txtKonto.Text = red["konto"].ToString();
                txtNaziv.Text = red["naziv"].ToString();
                txtPrenos.Text = red["prenos"].ToString();
            }
        }

        private void IzvrsiUpit(string sql, string poruka)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    UcitajPodatke();
                    MessageBox.Show(poruka, "Obaveštenje");
                }
            }
            catch (Exception ex) { MessageBox.Show("SQL Greška: " + ex.Message); }
        }

        // Pomoćna metoda za čišćenje inputa
        private void OcistiSve()
        {
            txtKonto.Clear();
            txtNaziv.Clear();
            txtPrenos.Clear();
            trenutniId = -1;
            dgKonto.SelectedItem = null;
        }
    }
}