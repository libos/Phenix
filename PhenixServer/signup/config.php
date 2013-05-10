<?php
$con = mysql_connect("localhost","root","jknlff8-pro-17m7755");
if (!$con)
{
  die('Could not connect: ' . mysql_error());
}

mysql_select_db("Phenix", $con);


//$result = mysql_query("SELECT count(*) FROM counter");
// if($result < 1)
// {
//   mysql_query("INSERT INTO counter (id, filename,count) VALUES (NULL, 'file1',0)");
//   mysql_query("INSERT INTO counter (id, filename,count) VALUES (NULL, 'file2',0)");
//   mysql_query("INSERT INTO counter (id, filename,count) VALUES (NULL, 'file3',0)");  
// }

//  $result = mysql_query("SELECT * FROM counter");
 // while($row = mysql_fetch_array($result))
 // {
 //   echo $row['count'] . "<br/>";
 // }
// mysql_query("UPDATE counter SET count = " . ($row['count']+1) . " WHERE id = " . $row['id']);
// echo "UPDATE counter SET count = " . ($row['count']+1) . " WHERE id = " . strval($row['id']);
// mysql_close($conn);
// echo "done!"

?>