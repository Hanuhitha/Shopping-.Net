using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Quickart_API.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Quickart_API.Controllers
{
    [Route("Order")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly IConfiguration _configuration;

        public OrderController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string validate(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:Token").Value);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                string Email = (jwtToken.Claims.First(x => x.Type == "email").Value).ToString();
                string UserID = (jwtToken.Claims.First(x => x.Type == "UserID").Value).ToString();

                return UserID;
            }
            catch (Exception e)
            {
                return null;
            }

        }


        [HttpPost("PlaceOrder", Name = nameof(PlaceOrderAsync))]
        public async Task<ActionResult<PlaceOrderResponse>> PlaceOrderAsync([FromBody] PlaceOrderRequest request)
        {
            try
            {
                string validate_token = validate(request.token);
                bool validUser = true;
                if (validate_token == null)
                {
                    validUser = false;
                    var response = new PlaceOrderResponse
                    {
                        response_code = 500
                    };

                    return response;
                }
                else
                {
                    List<OrderDetails> order_list = new List<OrderDetails>();
                    order_list = request.orderDetails;
                    DataTable table;
                    table = new DataTable();
                    string DataSource = _configuration.GetConnectionString("QuickartCon");
                    MySqlDataReader myReader;

                    string storeId = request.storeId;
                    string st = "Insert into quickart_db.orders (order_placed_date, purchase_type, order_status_id, user_id, delivery_person, address_type, phone_number, store_id, payment_reference) values ('" + request.date + "','" + request.purchaseType + "'," + 1 + "," + validate_token + "," + 0 + ",'" + request.address + "','" + request.cellNumber + "','" + storeId + "','" + request.paymentReference + "')";

                    foreach (var order in order_list)
                    {
                        using (MySqlConnection mycon = new MySqlConnection(DataSource))
                        {
                            table = new DataTable();
                            mycon.Open();
                            using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                            {
                                myReader = mycommand.ExecuteReader();
                                table.Load(myReader);

                                myReader.Close();
                                mycon.Close();
                            }
                        }

                        foreach (var product in order.products)
                        {
                            String prdId = product.product_id;
                            int prdQty = product.product_quantity;
                            string st2 = "Insert into quickart_db.ordered_items (select max(o.order_id), sp.store_product_id," + prdQty + ", p.product_price from orders o, store_product sp, products p where o.store_id = sp.store_id and sp.product_id ='" + prdId + "' and p.product_id ='" + prdId + "')";


                            using (MySqlConnection mycon = new MySqlConnection(DataSource))
                            {
                                table = new DataTable();
                                mycon.Open();
                                using (MySqlCommand mycommand = new MySqlCommand(st2, mycon))
                                {
                                    myReader = mycommand.ExecuteReader();
                                    table.Load(myReader);

                                    myReader.Close();
                                    mycon.Close();
                                }
                            }

                        }
                    }

                    var response = new PlaceOrderResponse
                    {
                        response_code = 200,
                        response_message = ""
                    };

                    return response;

                }
            }
            catch (Exception e)
            {
                var response = new PlaceOrderResponse
                {
                    response_code = 404,
                    response_message = e.ToString()
                };

                return response;
            }

        }


        [HttpPost("OrderHistory", Name = nameof(OrderHistoryAsync))]
        public async Task<ActionResult<OrderHistoryResponse>> OrderHistoryAsync([FromBody] OrderHistoryRequest request)
        {
            try
            {
                string validate_token = validate(request.token);
                bool validUser = true;
                if (validate_token == null)
                {
                    validUser = false;
                    var response = new OrderHistoryResponse
                    {
                        response_code = 500
                    };

                    return response;
                }
                else
                {
                    int userId = Convert.ToInt32(validate_token);
                    List<int> orderIds = new List<int>();
                    string st;
                    var response = new OrderHistoryResponse();
                    st = "select order_id from orders where user_id=" + userId + " order by order_id desc";
                    DataTable table = new DataTable();
                    string DataSource = _configuration.GetConnectionString("QuickartCon");
                    MySqlDataReader myReader;
                    using (MySqlConnection mycon = new MySqlConnection(DataSource))
                    {
                        mycon.Open();
                        using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                        {
                            myReader = mycommand.ExecuteReader();
                            table.Load(myReader);
                            myReader.Close();
                            mycon.Close();
                        }

                    }

                    foreach (DataRow row in table.Rows)
                    {
                        orderIds.Add(Convert.ToInt32(row["order_id"]));
                    }

                    List<Datum> data = new List<Datum>();
                    foreach (int Id in orderIds)
                    {
                        Datum order = new Datum();
                        st = "select s.store_name, s.store_image, s.store_address, o.order_placed_date, o.purchase_type, ost.status from quickart_db.orders o, quickart_db.stores s, quickart_db.order_status_types ost where o.order_id = " + Id + " and o.store_id = s.store_id and ost.order_status_id = o.order_status_id;";
                        table = new DataTable();
                        using (MySqlConnection mycon = new MySqlConnection(DataSource))
                        {
                            mycon.Open();
                            using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                            {
                                myReader = mycommand.ExecuteReader();
                                table.Load(myReader);
                                myReader.Close();
                                mycon.Close();
                            }

                            foreach (DataRow row in table.Rows)
                            {
                                order.storeName = row["store_name"].ToString();
                                order.storeImg = row["store_image"].ToString();
                                order.storeAddress = row["store_address"].ToString();
                                order.orderDate = row["order_placed_date"].ToString();
                                order.orderType = row["purchase_type"].ToString();
                                order.orderStatus = row["status"].ToString();
                                order.orderId = Id;
                            }
                        }


                        // OrderValue, NoOfProducts
                        st = "select sum(product_qty_cnt) as no_of_products, sum(product_qty_cnt*product_price) as order_value from quickart_db.ordered_items where order_id = " + Id + ";";
                        table = new DataTable();
                        using (MySqlConnection mycon = new MySqlConnection(DataSource))
                        {
                            mycon.Open();
                            using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                            {
                                myReader = mycommand.ExecuteReader();
                                table.Load(myReader);
                                myReader.Close();
                                mycon.Close();
                            }

                            foreach (DataRow row in table.Rows)
                            {
                                if (row["no_of_products"] != DBNull.Value) order.noOfProducts = Convert.ToInt32(row["no_of_products"]);
                                if (row["order_value"] != DBNull.Value) order.orderValue = Convert.ToDouble(row["order_value"]);
                            }
                        }

                        //list of products
                        List<OrderProduct> ops = new List<OrderProduct>();
                        st = "select sp.product_id, o_items.product_qty_cnt, p.product_price, p.product_name, p.product_image_url, p.product_weight from quickart_db.ordered_items o_items join quickart_db.store_product sp on o_items.store_product_id = sp.store_product_id join quickart_db.products p on sp.product_id=p.product_id where o_items.order_id = " + Id + ";";
                        table = new DataTable();
                        using (MySqlConnection mycon = new MySqlConnection(DataSource))
                        {
                            mycon.Open();
                            using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                            {
                                myReader = mycommand.ExecuteReader();
                                table.Load(myReader);
                                myReader.Close();
                                mycon.Close();
                            }

                            foreach (DataRow row in table.Rows)
                            {
                                OrderProduct op = new OrderProduct();
                                op.productId = row["product_id"].ToString();
                                if (row["product_qty_cnt"] != DBNull.Value)
                                    op.productQtyCnt = Convert.ToInt32(row["product_qty_cnt"]);
                                if (row["product_price"] != DBNull.Value)
                                    op.productPrice = Convert.ToDouble(row["product_price"]);
                                op.productName = row["product_name"].ToString();
                                op.productImageUrl = row["product_image_url"].ToString();
                                op.productWeight = row["product_weight"].ToString();

                                ops.Add(op);
                            }
                        }
                        order.orderProducts = ops;

                        data.Add(order);

                    }
                    response.data = data;
                    response.response_code = 200;
                    response.response_message = "returned";

                    return response;

                }

            }
            catch (Exception e)
            {
                var response = new OrderHistoryResponse
                {
                    response_code = 404,
                    response_message = e.ToString()
                };
                return response;
            }

        }


        [HttpPost("ChangeOrderStatus", Name = nameof(ChangeOrderStatusAsync))]
        public async Task<ActionResult<ChangeOrderStatusResponse>> ChangeOrderStatusAsync([FromBody] ChangeOrderStatusRequest request)
        {
            try
            {
                string validate_token = validate(request.token);
                bool validUser = true;
                if (validate_token == null)
                {
                    validUser = false;
                    var response = new ChangeOrderStatusResponse
                    {
                        response_code = 500
                    };

                    return response;
                }
                else
                {
                    //string st = "update orders set order_status_id=(select order_status_id from order_status_types where status='" + request.order_status + "') where order_id = '" + request.order_id + "'";
                    DataTable table = new DataTable();
                    string DataSource = _configuration.GetConnectionString("QuickartCon");
                    MySqlDataReader myReader;
                    string st;

                    using (MySqlConnection mycon = new MySqlConnection(DataSource))
                    {
                        st = "update orders set order_status_id=(select order_status_id from order_status_types where status='" + request.order_status + "') where order_id = '" + request.order_id + "'";
                        mycon.Open();
                        using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                        {
                            myReader = mycommand.ExecuteReader();
                            table.Load(myReader);

                            myReader.Close();
                            mycon.Close();
                        }
                    }

                    var response = new ChangeOrderStatusResponse
                    {
                        response_code = 200,
                        response_message = ""
                    };

                    return response;
                }

            }
            catch (Exception e)
            {
                var response = new ChangeOrderStatusResponse
                {
                    response_code = 404,
                    response_message = e.ToString()
                };

                return response;
            }
        }


        [HttpPost("AssignedOrders", Name = nameof(AssignedOrdersAsync))]
        public async Task<ActionResult<AssignedOrdersResponse>> AssignedOrdersAsync([FromBody] AssignedOrdersRequest request)
        {
            try
            {
                string validate_token = validate(request.token);
                bool validUser = true;
                if (validate_token == null)
                {
                    validUser = false;
                    var response = new AssignedOrdersResponse
                    {
                        response_code = 500
                    };

                    return response;
                }
                else
                {
                    int userId = Convert.ToInt32(validate_token);
                    List<int> orderIds = new List<int>();
                    string st;
                    var response = new AssignedOrdersResponse();
                    st = "select order_id from orders where delivery_person=" + userId + " and order_status_id in (2, 3);";
                    DataTable table = new DataTable();
                    string DataSource = _configuration.GetConnectionString("QuickartCon");
                    MySqlDataReader myReader;
                    using (MySqlConnection mycon = new MySqlConnection(DataSource))
                    {
                        mycon.Open();
                        using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                        {
                            myReader = mycommand.ExecuteReader();
                            table.Load(myReader);
                            myReader.Close();
                            mycon.Close();
                        }

                    }

                    foreach (DataRow row in table.Rows)
                    {
                        orderIds.Add(Convert.ToInt32(row["order_id"]));
                    }

                    List<AssignedOrders> data = new List<AssignedOrders>();

                    foreach (int Id in orderIds)
                    {
                        AssignedOrders order = new AssignedOrders();
                        table = new DataTable();
                        st = "select s.store_name, s.store_image, s.store_address, o.order_placed_date, o.purchase_type, ost.status, ua.address from quickart_db.orders o, quickart_db.stores s, quickart_db.order_status_types ost, quickart_db.user_address ua where o.order_id = " + Id + " and o.store_id = s.store_id and ost.order_status_id = o.order_status_id and o.user_id = ua.user_id and o.address_type = ua.Address_Type;";
                        using (MySqlConnection mycon = new MySqlConnection(DataSource))
                        {
                            mycon.Open();
                            using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                            {
                                myReader = mycommand.ExecuteReader();
                                table.Load(myReader);
                                myReader.Close();
                                mycon.Close();
                            }

                            foreach (DataRow row in table.Rows)
                            {
                                order.storeName = row["store_name"].ToString();
                                order.storeImg = row["store_image"].ToString();
                                order.storeAddress = row["store_address"].ToString();
                                order.orderDate = row["order_placed_date"].ToString();
                                order.orderType = row["purchase_type"].ToString();
                                order.orderStatus = row["status"].ToString();
                                order.Address = row["address"].ToString();
                                order.orderId = Id;
                            }
                        }


                        st = "select sum(product_qty_cnt) as no_of_products, sum(product_qty_cnt*product_price) as order_value from quickart_db.ordered_items where order_id = " + Id + ";";
                        table = new DataTable();
                        using (MySqlConnection mycon = new MySqlConnection(DataSource))
                        {
                            mycon.Open();
                            using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                            {
                                myReader = mycommand.ExecuteReader();
                                table.Load(myReader);
                                myReader.Close();
                                mycon.Close();
                            }

                            foreach (DataRow row in table.Rows)
                            {
                                if (row["no_of_products"] != DBNull.Value) order.noOfProducts = Convert.ToInt32(row["no_of_products"]);
                                if (row["order_value"] != DBNull.Value) order.orderValue = Convert.ToDouble(row["order_value"]);
                            }
                        }


                        List<ProductDetails> ops = new List<ProductDetails>();
                        table = new DataTable();
                        st = "select sp.product_id, o_items.product_qty_cnt, p.product_price, p.product_name, p.product_image_url, p.product_weight from ordered_items o_items join store_product sp on o_items.store_product_id = sp.store_product_id  join products p ON p.product_id = sp.product_id where o_items.order_id =" + Id + " ;";
                        using (MySqlConnection mycon = new MySqlConnection(DataSource))
                        {
                            mycon.Open();
                            using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                            {
                                myReader = mycommand.ExecuteReader();
                                table.Load(myReader);
                                myReader.Close();
                                mycon.Close();
                            }


                            foreach (DataRow row in table.Rows)
                            {
                                ProductDetails op = new ProductDetails();
                                op.productId = row["product_id"].ToString();
                                if (row["product_qty_cnt"] != DBNull.Value)
                                    op.productQtyCnt = Convert.ToInt32(row["product_qty_cnt"]);
                                if (row["product_price"] != DBNull.Value)
                                    op.productPrice = Convert.ToDouble(row["product_price"]);
                                op.productName = row["product_name"].ToString();
                                op.productImageUrl = row["product_image_url"].ToString();
                                op.productWeight = row["product_weight"].ToString();
                                ops.Add(op);
                            }

                            order.orderProducts = ops;
                        }

                        data.Add(order);

                    }
                    response.AssignedOrders = data;
                    response.response_code = 200;
                    response.response_message = "returned";

                    return response;
                }
            }
            catch (Exception e)
            {
                var response = new AssignedOrdersResponse
                {
                    response_code = 404,
                    response_message = e.ToString()
                };
                return response;
            }

        }


        [HttpPost("Orders", Name = nameof(OrdersAsync))]
        public async Task<ActionResult<OrdersResponse>> OrdersAsync([FromBody] OrdersRequest request)
        {
            try
            {
                string validate_token = validate(request.token);
                bool validUser = true;
                if (validate_token == null)
                {
                    validUser = false;
                    var response = new OrdersResponse
                    {
                        response_code = 500
                    };

                    return response;
                }
                else
                {
                    int usrId = Convert.ToInt32(validate_token);
                    List<DeliveryPerson> dp_list = new List<DeliveryPerson>();
                    List<Order> ord_list = new List<Order>();
                    string st;
                    DataTable table;
                    string DataSource = _configuration.GetConnectionString("QuickartCon");
                    MySqlDataReader myReader;

                    using (MySqlConnection mycon = new MySqlConnection(DataSource))
                    {
                        // Delivery Persons
                        table = new DataTable();
                        st = "select user_id, concat(first_name,' ',last_name) as name from users where user_type='Delivery Person'; ";
                        mycon.Open();
                        using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                        {
                            myReader = mycommand.ExecuteReader();
                            table.Load(myReader);

                            myReader.Close();
                            mycon.Close();
                        }

                        foreach (DataRow row in table.Rows)
                        {
                            DeliveryPerson dp = new DeliveryPerson();
                            if (row["user_id"] != DBNull.Value) dp.user_id = Convert.ToInt32(row["user_id"]);
                            dp.name = row["name"].ToString();
                            dp_list.Add(dp);
                        }

                        // Order Details
                        table = new DataTable();
                        st = "select o.order_id, s.store_image, s.store_name, s.store_address, s.store_lat, s.store_long from quickart_db.orders o join quickart_db.stores s on o.store_id = s.store_id where o.order_status_id=1 ;";
                        mycon.Open();
                        using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                        {
                            myReader = mycommand.ExecuteReader();
                            table.Load(myReader);

                            myReader.Close();
                            mycon.Close();
                        }

                        foreach (DataRow row in table.Rows)
                        {
                            Order ord = new Order();
                            if (row["order_id"] != DBNull.Value)
                                ord.order_id = Convert.ToInt32(row["order_id"]);
                            ord.store_img = row["store_image"].ToString();
                            ord.store_name = row["store_name"].ToString();
                            ord.store_address = row["store_address"].ToString();
                            ord.store_address_lat = row["store_lat"].ToString();
                            ord.store_address_long = row["store_long"].ToString();

                            // Order Value, No of products
                            st = "select sum(product_qty_cnt) as no_of_products, sum(product_qty_cnt*product_price) as order_value from quickart_db.ordered_items where order_id="+ ord.order_id;
                            table = new DataTable();
                            mycon.Open();
                            using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                            {
                                myReader = mycommand.ExecuteReader();
                                table.Load(myReader);
                                myReader.Close();
                                mycon.Close();
                            }
                            foreach (DataRow row1 in table.Rows)
                            {
                                ord.order_value = Convert.ToDouble(row1["order_value"]); ;
                                ord.total_items = Convert.ToInt32(row1["no_of_products"]);
                            }


                            // Delivery Address details
                            st = "select ua.address, ua.apt_suit_no, ua.lat, ua.longitude from quickart_db.orders o join quickart_db.user_address ua on o.user_id = ua.user_id and o.address_type = ua.Address_Type and order_id ="+ord.order_id;
                            table = new DataTable();
                            mycon.Open();
                            using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                            {
                                myReader = mycommand.ExecuteReader();
                                table.Load(myReader);
                                myReader.Close();
                                mycon.Close();
                            }
                            foreach (DataRow row1 in table.Rows)
                            {
                                ord.delivery_address = Convert.ToString(row1["address"]);
                                ord.apt_number = Convert.ToString(row1["apt_suit_no"]) ;
                                ord.delivery_add_lat = Convert.ToString(row1["lat"]);
                                ord.delivery_add_long = Convert.ToString(row1["longitude"]);
                            }


                            ord_list.Add(ord);
                        }


                    }
                    var response = new OrdersResponse
                    {
                        response_code = 200,
                        response_message = "Returned",
                        delivery_people = dp_list,
                        orders = ord_list
                    };
                    return response;
                }
            }
            catch (Exception e)
            {
                var response = new OrdersResponse
                {
                    response_code = 404,
                    response_message = e.ToString()
                };

                return response;
            }
        }


        [HttpPost("ModifyOrder", Name = nameof(ModifyOrderAsync))]
        public async Task<ActionResult<ModifyOrderResponse>> ModifyOrderAsync([FromBody] ModifyOrderRequest request)
        {
            try
            {
                string validate_token = validate(request.token);
                bool validUser = true;
                if (validate_token == null)
                {
                    validUser = false;
                    var response = new ModifyOrderResponse
                    {
                        response_code = 500
                    };

                    return response;
                }
                else
                {
                    int userId = request.user_id;
                    List<int> orderIds = new List<int>();
                    orderIds = request.order_ids;
                    string DataSource = _configuration.GetConnectionString("QuickartCon");
                    MySqlDataReader myReader;
                    foreach (int Id in orderIds)
                    {
                        string st = "update quickart_db.orders set order_status_id=2, delivery_person="+ userId +" where order_id=" + Id;
                        using (MySqlConnection mycon = new MySqlConnection(DataSource))
                        {
                            mycon.Open();
                            using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                            {
                                myReader = mycommand.ExecuteReader();
                            }
                            mycon.Close();
                        }
                    }

                    var response = new ModifyOrderResponse
                    {
                        response_code = 200,
                        reponse_message = "changed"
                    };
                    return response;
                }
            }
            catch (Exception e)
            {
                var response = new ModifyOrderResponse
                {
                    response_code = 404,
                    reponse_message = e.ToString()
                };
                return response;
            }
        }

    }
}
