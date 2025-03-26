using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Enigma_Machine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Constants for rotor wiring and reflector
        string _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Standard alphabet for reference
        string _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV"; // Rotor 1 wiring (Hours)
        string _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX"; // Rotor 2 wiring (Minutes)
        string _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA"; // Rotor 3 wiring (Seconds)
        string _reflector = "YRUHQSLDPXNGOKMIEBFZCWVJAT"; // Reflector wiring

        // Rotor offset tracking
        int[] _keyOffset = { 0, 0, 0 }; // Current rotor offsets (H, M, S)
        int[] _initOffset = { 0, 0, 0 }; // Initial rotor offsets (H, M, S)

        // Rotor state flag
        bool _rotor = false;

        // Plugboard setup
        Dictionary<char, char> _plugboard = new Dictionary<char, char>(); // Plugboard dictionary
        private bool _plugboardSet = false; // Flag to indicate if plugboard is set

        // Constructor
        public MainWindow()
        {
            InitializeComponent();

            SetDefaults(); // Initialize default values

            string[] rings = { _ring1, _ring2, _ring3 };
            StackPanel[] ringPanels = { Ring1, Ring2, Ring3 };
            DisplayRotor(rings, ringPanels);
            //DisplayReflector();

            _rotor = false; // Initially rotor is off
            btnRotor.Content = "Rotor On"; // Set button text
            btnRotor.IsEnabled = false; // Disable rotor button until plugboard is set
        }

        // Display rotor wiring in UI labels
        private void DisplayRing(Label displayLabel, string ring)
        {
            string temp = "";
            foreach (char r in ring)
                temp += r + "\t"; // Add tab for spacing
            displayLabel.Content = temp;
        }

        // Find the index of a character in a string
        private int IndexSearch(string ring, char letter)
        {
            int index = 0;
            for (int x = 0; x < ring.Length; x++)
            {
                if (ring[x] == letter)
                {
                    index = x;
                    break;
                }
            }
            return index;
        }

        // Handle keyboard input
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_plugboardSet) //only if the pluboard is set already
            {
                // Check for uppercase letters and message length
                //if (e.Key.ToString().Length == 1 && lblInput.Text.ToString().Length < 128)
                if (e.Key.ToString().Length == 1)
                {
                    if ((int)e.Key.ToString()[0] >= 65 && (int)e.Key.ToString()[0] <= 90)
                    {
                        lblInput.Text += e.Key.ToString(); // Append input character
                        lblEncrpyt.Text += Encrypt(e.Key.ToString()[0]) + ""; // Encrypt and append
                        lblEncrpytMirror.Text += Mirror(e.Key.ToString()[0]) + ""; // Mirror and append

                        LightUpKey();
                        TurnRotor();

                        // Rotate rotors if enabled
                        if (_rotor)
                        {
                            Rotate(true);
                            DisplayRing(lblRing1, _ring1); // Update rotor display
                            DisplayRing(lblRing2, _ring2);
                            DisplayRing(lblRing3, _ring3);
                        }
                    }
                }
                // Handle space key
                else if (e.Key == Key.Space)
                {
                    lblInput.Text += " ";
                    lblEncrpyt.Text += " ";
                    lblEncrpytMirror.Text += " ";
                    TurnRotor();
                }
                // Handle backspace key
                else if (e.Key == Key.Back)
                {
                    Rotate(false); // Rotate rotors backward
                    DisplayRing(lblRing1, _ring1);
                    DisplayRing(lblRing2, _ring2);
                    DisplayRing(lblRing3, _ring3);

                    lblInput.Text = RemoveLastLetter(lblInput.Text.ToString()); // Remove last character
                    lblEncrpyt.Text = RemoveLastLetter(lblEncrpyt.Text.ToString());
                    lblEncrpytMirror.Text = RemoveLastLetter(lblEncrpytMirror.Text.ToString());
                    TurnRotor();
                }
            }
        }

        // Encrypt a character
        private char Encrypt(char letter)
        {
            char newChar = letter;

            // Plugboard pass (before rotors)
            if (_plugboard.ContainsKey(newChar))
                newChar = _plugboard[newChar];
            else if (_plugboard.ContainsValue(newChar))
                newChar = _plugboard.FirstOrDefault(x => x.Value == newChar).Key;

            // Rotor pass forward
            newChar = _ring1[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring3[IndexSearch(_control, newChar)];

            // Reflector pass
            newChar = _reflector[IndexSearch(_control, newChar)];
            Reflector.Text = newChar.ToString();

            // Rotor pass backward
            newChar = _ring3[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring1[IndexSearch(_control, newChar)];

            // Plugboard pass (after rotors)
            if (_plugboard.ContainsKey(newChar))
                newChar = _plugboard[newChar];
            else if (_plugboard.ContainsValue(newChar))
                newChar = _plugboard.FirstOrDefault(x => x.Value == newChar).Key;

            return newChar;
        }

        // Mirror a character (encrypt and pass back through rotors)
        private char Mirror(char letter)
        {
            char newChar = Encrypt(letter);

            newChar = _ring3[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring1[IndexSearch(_control, newChar)];

            return newChar;
        }

        // Set default values
        private void SetDefaults()
        {
            _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV";
            _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX";
            _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA";
            _keyOffset = new int[] { 0, 0, 0 };

            lblInput.Text = "";
            lblEncrpyt.Text = "";
            lblEncrpytMirror.Text = "";

            //DisplayRing(lblControlRing, _control);
            DisplayRing(lblRing1, _ring1);
            DisplayRing(lblRing2, _ring2);
            DisplayRing(lblRing3, _ring3);
        }

        // Rotate rotors
        private void Rotate(bool forward)
        {
            if (forward)
            {
                _keyOffset[2]++;
                _ring3 = MoveValues(forward, _ring3);

                if (_keyOffset[2] / _control.Length >= 1)
                {
                    _keyOffset[2] = 0;
                    _keyOffset[1]++;
                    _ring2 = MoveValues(forward, _ring2);
                    if (_keyOffset[1] / _control.Length >= 1)
                    {
                        _keyOffset[1] = 0;
                        _keyOffset[0]++;
                        _ring1 = MoveValues(forward, _ring1);
                    }
                }
            }
            else
            {
                if (_keyOffset[2] > 0 || _keyOffset[1] > 0)
                {
                    _keyOffset[2]--;
                    _ring3 = MoveValues(forward, _ring3);
                    if (_keyOffset[2] < 0)
                    {
                        _keyOffset[2] = 25;
                        _keyOffset[1]--;
                        _ring2 = MoveValues(forward, _ring2);
                        if (_keyOffset[1] < 0)
                        {
                            _keyOffset[1] = 25;
                            _keyOffset[0]--;
                            _ring1 = MoveValues(forward, _ring1);
                            if (_keyOffset[0] < 0)
                                _keyOffset[0] = 25;
                        }
                    }
                }
            }

            DisplayOffset(); // Update offset display
        }

        // Move rotor values
        private string MoveValues(bool forward, string ring)
        {
            char movingValue = ' ';
            string newRing = "";

            if (forward)
            {
                movingValue = ring[0];
                for (int x = 1; x < ring.Length; x++)
                    newRing += ring[x];
                newRing += movingValue;
            }
            else
            {
                movingValue = ring[25];
                for (int x = 0; x < ring.Length - 1; x++)
                    newRing += ring[x];
                newRing = movingValue + newRing;
            }

            return newRing;
        }

        // Handle rotor button click
        private void btnRotor_Click(object sender, RoutedEventArgs e)
        {
            SetDefaults();

            if (int.TryParse(txtBRing1Init.Text, out _initOffset[0]) &&
                int.TryParse(txtBRing2Init.Text, out _initOffset[1]) &&
                int.TryParse(txtBRing3Init.Text, out _initOffset[2]))
            {
                if (_initOffset[0] >= 0 && _initOffset[0] <= 25 &&
                    _initOffset[1] >= 0 && _initOffset[1] <= 25 &&
                    _initOffset[2] >= 0 && _initOffset[2] <= 25)
                {
                    txtBRing1Init.IsEnabled = false;
                    txtBRing2Init.IsEnabled = false;
                    txtBRing3Init.IsEnabled = false;

                    _rotor = true;
                    btnRotor.Content = "Settings Lock";

                    _ring1 = InitializeRotors(_initOffset[0], _ring1);
                    _ring2 = InitializeRotors(_initOffset[1], _ring2);
                    _ring3 = InitializeRotors(_initOffset[2], _ring3);

                    DisplayRing(lblRing1, _ring1);
                    DisplayRing(lblRing2, _ring2);
                    DisplayRing(lblRing3, _ring3);
                    DisplayOffset();
                }
            }

            TurnRotor();
        }

        // Initialize rotors with initial offset
        private string InitializeRotors(int initial, string ring)
        {
            string newRing = ring;
            for (int x = 0; x < initial; x++)
                newRing = MoveValues(true, newRing);
            return newRing;
        }

        // Remove last letter from a string
        private string RemoveLastLetter(string word)
        {
            string newWord = "";
            for (int x = 0; x < word.Length - 1; x++)
                newWord += word[x];
            return newWord;
        }

        // Handle text box focus
        private void txtBRing1Init_GotFocus(object sender, RoutedEventArgs e)
        {
            txtBRing1Init.Text = "";
        }

        private void txtBRing2Init_GotFocus(object sender, RoutedEventArgs e)
        {
            txtBRing2Init.Text = "";
        }

        private void txtBRing3Init_GotFocus(object sender, RoutedEventArgs e)
        {
            txtBRing3Init.Text = "";
        }

        // Display rotor offsets
        private void DisplayOffset()
        {
            txtBRing1Init.Text = _keyOffset[0] + "";
            txtBRing2Init.Text = _keyOffset[1] + "";
            txtBRing3Init.Text = _keyOffset[2] + "";
        }

        // Setup plugboard
        private void SetupPlugboard(string plugboardSettings)
        {
            _plugboard.Clear();

            string[] pairs = plugboardSettings.ToUpper().Split(' ');
            foreach (string pair in pairs)
            {
                if (pair.Length == 2)
                {
                    _plugboard[pair[0]] = pair[1];
                    _plugboard[pair[1]] = pair[0];
                }
            }
        }

        private bool InPlugboardFormat()
        {
            if (txtPlugboard.Text.Length > 29)
                return false; //only 10 cables

            for (int i = 0; i < txtPlugboard.Text.Length; i++)
            {
                if (i % 3 == 2 && txtPlugboard.Text[i] != ' ') //check for spaces on the 3rd char
                    return false;

                if (i % 3 != 2 && !Char.IsLetter(txtPlugboard.Text[i]))
                    return false;
            }

            return true;
        }

        // Handle plugboard button click
        private void btnSetPlugboard_Click(object sender, RoutedEventArgs e)
        {
            if (!InPlugboardFormat())
            {
                MessageBox.Show("Plugboard is not in the correct format.");
                return;
            }

            if (_plugboardSet)
            {
                MessageBox.Show("Plugboard is already set.");
                return;
            }

            SetupPlugboard(txtPlugboard.Text);
            _plugboardSet = true;
            btnRotor.IsEnabled = true; // Enable the rotor button.

            // Explicitly check the flag and perform an action
            if (_plugboardSet)
            {
                txtPlugboard.IsEnabled = false;
            }
        }

        // Handle plugboard text change
        private void txtPlugboard_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblInput.Text = "";
            lblEncrpyt.Text = "";
            lblEncrpytMirror.Text = "";
        }

        //code for the plugboard
        private async void LightUpKey()
        {
            if (!string.IsNullOrEmpty(lblEncrpyt.Text.ToString()))
            {
                string encrypt = lblEncrpyt.Text.ToString();
                char lastKey = encrypt[encrypt.Length - 1]; //last character only

                Border lampButton = FindName(lastKey.ToString()) as Border;

                if (lampButton != null)
                {
                    lampButton.Background = Brushes.LightYellow; // light color
                    await Task.Delay(250); // 1/4 second
                    lampButton.Background = Brushes.DarkGray; // default color
                }
            }
        }

        private void TurnRotor() //it works???
        {
            Ring1.Children.Clear();
            Ring2.Children.Clear();
            Ring3.Children.Clear();

            //string[] rings = { lblRing1.Content.ToString(), lblRing2.Content.ToString(), lblRing3.Content.ToString() };
            string[] rings = { _ring1, _ring2, _ring3 };
            StackPanel[] ringPanels = { Ring1, Ring2, Ring3 };

            DisplayRotor(rings, ringPanels);
        }

        private void DisplayRotor(string[] rings, StackPanel[] ringPanels)
        {

            for (int r = 0; r < 3; r++) //rings 1-3
            {
                for (int i = -2; i <= 2; i++)
                {
                    bool middle = false;

                    if (i == 0)
                        middle = true;

                    Border border = new Border
                    {
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Background = Brushes.Gray,
                        CornerRadius = middle ? new CornerRadius(6) : new CornerRadius(3),
                        Height = middle ? 40 : 30,
                        Width = middle ? 40 : 30,
                        Margin = new Thickness(0, 2, 0, 1)
                    };

                    string txt = rings[r][(rings[r].Length + i) % rings[r].Length].ToString();

                    //MessageBox.Show(rings[0]);

                    //skip spaces
                    while (txt == " ")
                    {
                        txt = rings[r][(rings[r].Length + i + 1) % rings[r].Length].ToString();
                    }

                    TextBlock textBlock = new TextBlock
                    {
                        Text = txt,
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        FontSize = middle ? 20 : 16,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    border.Child = textBlock;

                    ringPanels[r].Children.Add(border);
                }
            }
        }

        private void NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _); //numbers only
        }

        private void LimitNumbers(object sender, RoutedEventArgs e) //text change event for the textboxes
        {
            if (sender is TextBox textBox && int.TryParse(textBox.Text, out int num))
            {
                if (num > 25)
                {
                    textBox.Text = "25";
                    textBox.CaretIndex = textBox.Text.Length; // Move cursor to the end
                }
            }
        }
    }
}