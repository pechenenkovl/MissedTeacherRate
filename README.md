# MissedTeacherRate

**MissedTeacherRate** is a WPF and console applications for analyzing and calculating missing marks from Excel-based rating matrices.

The application is designed for educational scenarios where both students and teachers evaluate works, and missing marks need to be identified, calculated, or estimated.

## Features

- **Excel Import**  
  Load rating matrices directly from `.xlsx` files.

- **Matrix Visualization**  
  View the parsed matrix in an Excel-like grid.

- **Algorithm Selection**  
  Choose from multiple missing mark calculation algorithms. Algorithms are discovered automatically via reflection if they implement `IMissedMarksCalculator`.

- **Result Comparison**  
  View the original matrix and the calculated matrix side by side for easier comparison.

## Usage

### 1. Open an Excel File

Click **Open Excel...** and select your `MarksMatrix.xlsx` file.

After the file is loaded, the matrix will be displayed in the main grid.

### 2. Select an Algorithm

Use the dropdown list to select a calculation algorithm.

Algorithms are discovered automatically if they implement the `IMissedMarksCalculator` interface.

### 3. Calculate Missing Marks

Click **Calculate** to process the matrix.

The calculated results will appear in the lower grid.

## Excel File Format

The input Excel file should follow this structure:

- The first row should contain student IDs from columns **B** to **N**.
- The last filled cell in the first row is treated as the teacher ID.
- The first column (**A**) should contain work IDs, for example: `D1`, `D2`, etc.
- Matrix cells should contain marks from **1** to **10**.
- Empty cells are treated as missing marks.

## Building the Project

### Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 or later

### Build Steps

1. Open the solution in Visual Studio 2022 or later.
2. Restore NuGet packages if needed.
3. Build the solution.
4. Run the `MissedTeacherRate.WPF` project.

## Project Structure

```text
MissedTeacherRate.WPF
└── WPF user interface and view models

MissedTeacherRate.Models
└── Data models

MissedTeacherRate.Parsers
└── Excel and matrix parsers

MissedTeacherRate.Algorithms
└── Missing mark calculation algorithms
```

## License

This project is intended for educational and non-commercial use.
