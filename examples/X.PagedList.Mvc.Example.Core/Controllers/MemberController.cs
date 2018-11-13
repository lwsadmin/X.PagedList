using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;

using System.Data.SqlTypes;
using System.Data.SqlClient;
using System;

namespace X.PagedList.Mvc.Example.Core.Controllers
{
    public class MemberController : Controller
    {

        public DataSet GetPaged(int pageIndex, int pageSize, string table, string orderBy, out int total)
        {
            total = 0;
            DataSet ds = new DataSet();
            string connectionString = "Server=localhost; Database=UnionMallDb; Trusted_Connection=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand comm = new SqlCommand("Global_GetPaged", conn);
                comm.CommandTimeout = 60;
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.AddWithValue("@PageIndex", pageIndex);
                comm.Parameters.AddWithValue("@PageSize", pageSize);
                comm.Parameters.AddWithValue("@Table", table);
                comm.Parameters.AddWithValue("@OrderBy", orderBy);
                comm.Parameters.Add("@TotalCount", SqlDbType.BigInt, 10);
                comm.Parameters["@TotalCount"].Direction = ParameterDirection.Output;
                comm.Parameters.Add("@Descript", SqlDbType.VarChar, 500);
                comm.Parameters["@Descript"].Direction = ParameterDirection.Output;
                SqlDataAdapter sda = new SqlDataAdapter(comm);
                sda.Fill(ds);
                if (comm.Parameters["@Descript"].Value.ToString() != "successful")
                {
                    throw new Exception(comm.Parameters["@Descript"].Value.ToString());
                }
                int.TryParse(comm.Parameters["@TotalCount"].Value.ToString(), out total);
            }
            return ds;
        }
        public IActionResult AjaxList(int page = 1)
        {

            string table = "select FullName from tmember ";
            int total;
            DataSet ds = GetPaged(page, 10, table, "", out total);
            IPagedList pageList = new PagedList<DataRow>(ds.Tables[0].Select(), page, 10, total);
            ViewBag.Names = pageList;

            return View();
        }

        public IActionResult Table(int page = 1, string name = "")
        {
            string table = "select FullName from tmember ";
            if (!string.IsNullOrEmpty(name))
            {
                table += $" where FullName like '%{name}%'";
            }
            int total;
            DataSet ds = GetPaged(page, 10, table, "", out total);
            IPagedList pageList = new PagedList<DataRow>(ds.Tables[0].Select(), page, 10, total);
            ViewBag.Names = pageList;
            return PartialView("_Table", pageList);
        }
    }
}