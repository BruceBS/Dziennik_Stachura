using Dziennik_BS.Data;
using System.Collections.ObjectModel;

namespace Dziennik_BS
{
    public partial class MainPage : ContentPage
    {

        ObservableCollection<Student> students = new ObservableCollection<Student>();
        int currentId = 1;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void AddStudentButton_Clicked(object sender, EventArgs e)
        {
            var result = await DisplayPromptAsync("Dodaj ucznia", "Wprowadź imię:", "OK", "Anuluj", placeholder: "Imię");

            if (result != null)
            {
                string firstName = result;

                result = await DisplayPromptAsync("Dodaj ucznia", "Wprowadź nazwisko:", "OK", "Anuluj", placeholder: "Nazwisko");

                if (result != null)
                {
                    string lastName = result;
                    Student newStudent = new Student(currentId, firstName, lastName);
                    students.Add(newStudent);
                    var studentLabel = new Label { Text = $"{currentId}. {firstName} {lastName}" };
                    studentsStackLayout.Children.Add(studentLabel);
                    currentId++;
                }
            }
        }

        private async void CreateClassButton_Clicked(object sender, EventArgs e)
        {
            string fileName = await DisplayPromptAsync("Nazwa pliku", "Podaj nazwę pliku:", "OK", "Anuluj", placeholder: "Nazwa pliku");

            if (string.IsNullOrWhiteSpace(fileName))
            {
                await DisplayAlert("Błąd", "Musisz podać nazwę pliku.", "OK");
                return;
            }

            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Class");
            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, $"{fileName}.txt");

            var studentsInfo = students.Select(student => $"{student.Id},{student.FirstName},{student.LastName}");

            try
            {
                await File.WriteAllLinesAsync(filePath, studentsInfo);

                await DisplayAlert("Sukces", $"Lista  została zapisana do pliku: {filePath}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Wystąpił błąd podczas zapisywania pliku: {ex.Message}", "OK");
            }
        }


        private async void EditStudentButton_Clicked(object sender, EventArgs e)
        {
            if (students.Count == 0)
            {
                await DisplayAlert("Nie ma żadnych uczniów na liście"," ", "OK");
                return;
            }

            string[] studentNames = students.Select(student => $"{student.FirstName} {student.LastName}").ToArray();

            string selectedStudent = await DisplayActionSheet("Wybierz ucznia, którego chcesz edytować", "Anuluj", null, studentNames);

            if (string.IsNullOrEmpty(selectedStudent) || selectedStudent == "Anuluj")
                return;

            int selectedIndex = Array.IndexOf(studentNames, selectedStudent);
            Student selectedStudentObject = students[selectedIndex];

            string newFirstName = await DisplayPromptAsync("Edytuj imię", "Wprowadź nowe imię:", "Zapisz", "Anuluj", selectedStudentObject.FirstName);
            if (string.IsNullOrEmpty(newFirstName))
            {
                await DisplayAlert("Błąd", "Imię nie może być puste.", "Ok");
                return;
            }

            string newLastName = await DisplayPromptAsync("Edytuj nazwisko", "Wprowadź nowe nazwisko:", "Zapisz", "Anuluj", selectedStudentObject.LastName);
            if (string.IsNullOrEmpty(newLastName))
            {
                await DisplayAlert("Błąd", "Nazwisko nie może być puste.", "OK");
                return;
            }

            if (newFirstName == selectedStudentObject.FirstName && newLastName == selectedStudentObject.LastName)
                return;

            selectedStudentObject.FirstName = newFirstName;
            selectedStudentObject.LastName = newLastName;

            foreach (var child in studentsStackLayout.Children)
            {
                if (child is Label label && label.Text.Contains(selectedStudent))
                {
                    label.Text = $"{selectedStudentObject.Id}. {newFirstName} {newLastName}";
                    break;
                }
            }
        }

        private async void LoadClassButton_Clicked(object sender, EventArgs e)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = Path.Combine(documentsPath, "Class");

            if (!Directory.Exists(folderPath))
            {
                await DisplayAlert("Błąd", "Folder 'Class' nie istnieje.", "OK");
                return;
            }

            string[] files = Directory.GetFiles(folderPath);

            string selectedFileNameWithExtension = await DisplayActionSheet("Wybierz plik", "Anuluj", null, files.Select(Path.GetFileNameWithoutExtension).ToArray());
            string selectedFile = files.First(filePath => Path.GetFileNameWithoutExtension(filePath) == selectedFileNameWithExtension);



            if (string.IsNullOrEmpty(selectedFile) || selectedFile == "Anuluj")
                return;

            students.Clear();
            studentsStackLayout.Children.Clear();

            string filePath = Path.Combine(folderPath, selectedFile);

            try
            {
                string fileContent = await File.ReadAllTextAsync(filePath);

                string[] lines = fileContent.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');

                    if (parts.Length == 3 && int.TryParse(parts[0], out int id))
                    {
                        Student newStudent = new Student(id, parts[1], parts[2]);
                        students.Add(newStudent);

                        var studentLabel = new Label { Text = $"{id}. {parts[1]} {parts[2]}" };
                        studentsStackLayout.Children.Add(studentLabel);
                    }
                    else
                    {
                        await DisplayAlert("Błąd", "Nieprawidłowy format linii.", "OK");
                    }
                }
                currentId = students.Count;
                currentId++;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Wystąpił błąd podczas wczytywania pliku: {ex.Message}", "OK");
            }
        }


        private async void PickStudentButton_Clicked(object sender, EventArgs e)
        {
            if (students.Count == 0)
            {
                await DisplayAlert("Brak uczniów", "Nie ma żadnych uczniów na liście.", "OK");
                return;
            }

            Random random = new Random();
            int randomIndex = random.Next(0, students.Count);

            Student pickedStudent = students[randomIndex];

            await DisplayAlert("Wylosowany uczeń", $"Numer: {pickedStudent.Id}\nImię: {pickedStudent.FirstName}\nNazwisko: {pickedStudent.LastName}", "OK");
        }

    }
}

