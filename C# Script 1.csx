

// ============================================================
// CONFIGURATION
// ============================================================

// Original measure tables
var sourceTables = new[]
{
    "One Kattameya Overall Measures",
    "One Kattameya Bank Measures",
    "One Kattameya Cash Measures",
    "OK Bank-Wise Measures"
};

// Target compounds
var compounds = new[]
{
    "Rihana",
    "Skyline Katamya Compound",
    "Crystal Plaza Maadi Compound",
    "Zahra North Coast",
    "Degla Landmark"
};

// Original names
var sourceProjectName = "One Kattameya Compound";
var sourceName = "One Kattameya";
var sourceAbbreviation = "OK";


// ============================================================
// REPLACE VALUES IN DAX EXPRESSIONS
// ============================================================

string ReplaceInExpression(string expression, string compound)
{
    if (string.IsNullOrEmpty(expression))
        return expression;

    // Replace the full project name first
    // One Kattameya Compound -> Rihana
    expression = expression.Replace(
        sourceProjectName,
        compound
    );

    // Replace remaining One Kattameya references
    expression = expression.Replace(
        sourceName,
        compound
    );

    return expression;
}


// ============================================================
// REPLACE VALUES IN MEASURE NAMES
// ============================================================

string ReplaceInMeasureName(string measureName, string compound)
{
    if (string.IsNullOrEmpty(measureName))
        return measureName;

    // --------------------------------------------------------
    // IMPORTANT:
    //
    // OK -> Full compound name
    //
    // One Kattameya -> Full compound name
    //
    // One Kattameya Compound -> Full compound name
    // --------------------------------------------------------

    // Replace full name first
    measureName = measureName.Replace(
        sourceProjectName,
        compound
    );

    // Replace One Kattameya
    measureName = measureName.Replace(
        sourceName,
        compound
    );

    // Replace OK
    measureName = measureName.Replace(
        sourceAbbreviation,
        compound
    );

    return measureName;
}


// ============================================================
// TARGET TABLE NAME
// ============================================================

string GetTargetTableName(
    string sourceTableName,
    string compound
)
{
    if (sourceTableName == "One Kattameya Overall Measures")
        return compound + " Overall Measures";

    if (sourceTableName == "One Kattameya Bank Measures")
        return compound + " Bank Measures";

    if (sourceTableName == "One Kattameya Cash Measures")
        return compound + " Cash Measures";

    if (sourceTableName == "OK Bank-Wise Measures")
        return compound + " Bank-Wise Measures";

    return compound + " Measures";
}


// ============================================================
// MAIN PROCESS
// ============================================================

foreach (var sourceTableName in sourceTables)
{
    // --------------------------------------------------------
    // Find source table
    // --------------------------------------------------------

    var sourceTable = Model.Tables
        .FirstOrDefault(t => t.Name == sourceTableName);

    if (sourceTable == null)
    {
        Info(
            "Source table not found:\n" +
            sourceTableName
        );

        continue;
    }


    // --------------------------------------------------------
    // Create measures for every compound
    // --------------------------------------------------------

    foreach (var compound in compounds)
    {
        string targetTableName =
            GetTargetTableName(
                sourceTableName,
                compound
            );


        // ====================================================
        // CREATE TARGET TABLE
        // ====================================================

        var targetTable = Model.Tables
            .FirstOrDefault(t =>
                t.Name == targetTableName
            );

        if (targetTable == null)
        {
            targetTable =
                Model.AddCalculatedTable(
                    targetTableName,
                    "ROW(\"Column\", BLANK())"
                );
        }


        // ====================================================
        // COPY EVERY MEASURE
        // ====================================================

        foreach (var sourceMeasure in sourceTable.Measures)
        {
            // ------------------------------------------------
            // Create new measure name
            // ------------------------------------------------

            string newMeasureName =
                ReplaceInMeasureName(
                    sourceMeasure.Name,
                    compound
                );


            // ------------------------------------------------
            // Create new DAX
            // ------------------------------------------------

            string newExpression =
                ReplaceInExpression(
                    sourceMeasure.Expression,
                    compound
                );


            // =================================================
            // CHECK IF MEASURE ALREADY EXISTS
            // =================================================

            var existingMeasure =
                targetTable.Measures
                    .FirstOrDefault(m =>
                        m.Name == newMeasureName
                    );


            if (existingMeasure == null)
            {
                // --------------------------------------------
                // Create new measure
                // --------------------------------------------

                var newMeasure =
                    targetTable.AddMeasure(
                        newMeasureName,
                        newExpression
                    );


                // --------------------------------------------
                // Copy properties
                // --------------------------------------------

                newMeasure.FormatString =
                    sourceMeasure.FormatString;

                newMeasure.DisplayFolder =
                    sourceMeasure.DisplayFolder;

                newMeasure.Description =
                    sourceMeasure.Description;

                newMeasure.IsHidden =
                    sourceMeasure.IsHidden;
            }
            else
            {
                // --------------------------------------------
                // Update existing measure
                // --------------------------------------------

                existingMeasure.Expression =
                    newExpression;

                existingMeasure.FormatString =
                    sourceMeasure.FormatString;

                existingMeasure.DisplayFolder =
                    sourceMeasure.DisplayFolder;

                existingMeasure.Description =
                    sourceMeasure.Description;

                existingMeasure.IsHidden =
                    sourceMeasure.IsHidden;
            }
        }
    }
}


// ============================================================
// DONE
// ============================================================

