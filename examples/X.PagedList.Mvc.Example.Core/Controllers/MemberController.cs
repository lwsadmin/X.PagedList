using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;

using System.Data.SqlTypes;
using System.Data.SqlClient;
namespace X.PagedList.Mvc.Example.Core.Controllers
{
    public class MemberController : Controller
    {
        private DataSet ExecuteDataSet(string sql, params object[] parameters)
        {
            string connectionString = "Server=localhost; Database=Sjlm; Trusted_Connection=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(sql, connection);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                // 填充DataSet. 
                da.Fill(ds);
                cmd.Parameters.Clear();
                connection.Close();
                return ds;

            }

        }
        public IActionResult AjaxList(int page = 1)
        {

            DataSet ds = ExecuteDataSet("select m.Name from tmember m where name!=''", null);

            IPagedList pageList = new PagedList<DataRow>(ds.Tables[0].Select(), page, 10);
            ViewBag.Names = pageList;

            return View();
        }

        public IActionResult Table(int page = 1, string name = "")
        {
            string sql = $"select m.Name from tmember m where name!='{name}'";


            if (!string.IsNullOrEmpty(name))
            {
                sql += $" and name like'%{name}%'";
            }
            DataSet ds = ExecuteDataSet(sql, null);


            PagedList<DataRow> pageList = new PagedList<DataRow>(ds.Tables[0].Select(), page, 10);
            pageList.TotalItemCount = 1000;
            pageList.PageCount = 1000 / 10;
            ViewBag.Names = pageList;
            return PartialView("_Table", pageList);
        }
    }
}