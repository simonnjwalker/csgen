using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class csvhandler
{
    public System.Data.DataTable GetDataTableFromCsv(string csvtext)
    {
        System.Data.DataTable dt = new System.Data.DataTable();
        dt.TableName = "csv";

        // assume the top row is header names
        string[] rows = csvtext.Split(new char[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries);
        string[] columnnames = rows[0].Split(new char[]{','},StringSplitOptions.None);
        int columncount = columnnames.Length;
        int startrow = 1;
        int rowcount = rows.Length;
        foreach(string columnname in columnnames)
        {
            System.Data.DataColumn column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = columnname;
            column.DefaultValue = "";
            dt.Columns.Add(column);
        }
        // start at the SECOND row because we have mapped the column-names already
        for(int thisrow = startrow; thisrow < rowcount; thisrow++)
        {
            System.Data.DataRow newrow = dt.NewRow();
            int maxcols = columncount;
            // 2019-05-20 SNJW okay what the fuck is the cell count DOING??
            // set a reasonable limit instead FML
            string[] rowvalues = rows[thisrow].Split(new char[]{','},StringSplitOptions.None);
            for (int thiscolumn = 0; thiscolumn< maxcols; thiscolumn++)
            {
                newrow[thiscolumn] = rowvalues[thiscolumn];
            }
            dt.Rows.Add(newrow);

        }
        return dt;
    }



    public byte[] GetCsvFromDataSet(System.Data.DataSet ds)
    {
        // TO DO
        byte[] output;
        using (MemoryStream mem = new MemoryStream())
        {
            output = mem.ToArray();
        }
        if(output == null)
            return new byte[]{};
        return output;
    }

    public void CreateCsvFileFromDataSet(System.Data.DataSet ds, string csvfile)
    {
        // TO DO
    }
    public System.Data.DataSet GetDataSetFromCsv(string csvfile)
    {
        System.Data.DataSet output = new System.Data.DataSet();
        bool success = true;
        string csvtext = "";
        try
        {
            csvtext = System.IO.File.ReadAllText(csvfile);
        }
        catch
        {
            success = false;
        }
        if(success)
        {
            System.Data.DataTable dt = this.GetDataTableFromCsv(csvtext);
            dt.TableName = System.IO.Path.GetFileNameWithoutExtension(csvfile);
            output.Tables.Add(dt);
        }
        return output;
    }

    public System.Data.DataSet GetDataSetFromCsv(byte[] csv)
    {
        // TO DO
        System.Data.DataSet output = new System.Data.DataSet();
        return output;
    }
}
