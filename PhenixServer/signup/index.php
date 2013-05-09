  <?php session_start(); ?>
<html>
<title>
  分布式消息队列系统用户注册
</title>
<script src="jquery-1.9.1.min.js" type="text/javascript" charset="utf-8"></script>
<style type="text/css">
*{
  margin:auto;
  padding:auto;
}
.table{
	font-family:Arial, Helvetica, sans-serif;
	font-size:12px;
	color:#333;
	background-color:#E4E4E4;	

}
.table td{
	background-color:#F8F8F8;	
}
.table td input
{
  padding:8px;
}
#signup
{
  margin-top:120px;
}
.submit
{
  padding:8px;
  border: 1px solid #006;
  background: #9cf;
}
.title
{
  text-align:center;
}
#msg
{
  margin-top:120px;
	font-family:Arial, Helvetica, sans-serif;
	font-size:12px;
	color:#333;
	background-color:#E4E4E4;	
  width:400px;
  height:100px;
  padding-top:80px;
  text-align:center;
}
a
{
  text-decoration:none;
}
</style>

<body>
  <?php 
  
  if (isset($_POST['email']) && isset($_POST['password']) && isset($_POST['password_confirm']) && isset($_POST['captcha']))
  {
    $msg=array();
    if (!filter_var($_POST['email'], FILTER_VALIDATE_EMAIL)) 
    {
      array_push($msg,"邮箱格式错误！");
    }
    if ($_POST['password'] != $_POST['password_confirm']) {
      array_push($msg,"两次密码不一致！");
    }
    if (strlen($_POST['password'])<6 || strlen($_POST['password'])>26) {
      array_push($msg,"密码长度必须在6~26位之间！");
    }
    if (strpos($_POST['password'],' ') !== false ||strpos($_POST['password'],'(') !== false||strpos($_POST['password'],')') !== false || strpos($_POST['password'],',') !== false|| strpos($_POST['password'],'%') !== false || strpos($_POST['password'],'\'') !== false|| strpos($_POST['password'],'\"') !== false) {
        array_push($msg,"密码不能包含空格、括号、英文逗号、英文引号和百分号");
    }
    if (empty($_SESSION['6_letters_code'] ) || strcasecmp($_SESSION['6_letters_code'], $_POST['captcha']) != 0) {
      array_push($msg,"验证码错误！");
    }
    if (!empty($msg)) {
    ?>
    <div id="msg">
      <?php foreach ($msg as $varx) {
      ?>
        <p><?php echo $varx; ?></p>
      <?php
      } ?>
      <p><a href="index.php">返回重新注册</a></p>
    </div>
    <?php
    }
    else
    {
      include "config.php";
      $email = $_POST['email'];
      $result  = mysql_query("select * from users where email = '" . $email . "'" );
      if (!$result) {
          die('Invalid query: ' . mysql_error());
      }
      $row_num = mysql_num_rows($result);
      if ($row_num != 0) {
?>
      <div id="msg">
          <p>该邮箱已经使用，请<a href="index.php">返回重新注册</a></p>
      </div>
<?php
      }
      else
      {
          $salt = uniqid(mt_rand(), true);
          $password = md5(md5($_POST['password']) . 'Phenix' . $salt);
          $result  = mysql_query("INSERT INTO users (id, email,password,salt,lastip) VALUES (NULL, '" . $email . "', '" . $password . "','"  . $salt . "','" . $_SERVER['REMOTE_ADDR'] . "')");
          if (!$result) {
              die('Invalid query: ' . mysql_error());
          }
          else
          {
?>
          <div id="msg">
            <p>注册成功</p>
          </div>
<?php
          }
      }
    }
  } 
  else
  {
    ?>
  <div id="signup">
    <h2 class="title">用户注册</h2>
    <form action="index.php" method="post" accept-charset="utf-8" class="form">
      <table class="table" width="400" border="0" align="center" cellpadding="5" cellspacing="1" >
        <tr>
          <td align="right" valign="center">用户名</td>
          <td align="center"><input type="text" name="email" value="" id="email" size="50" class="required"></td>
        </tr>
        <tr>
          <td align="right" valign="center">输入密码</td>
          <td align="center"><input type="password" name="password" value="" id="password" size="50" class="required"></td>
        </tr>
        <tr>
          <td align="right" valign="center">确认密码</td>
          <td align="center"><input type="password" name="password_confirm" value="" id="password_confirm" size="50" class="required"></td>
        </tr>
        <tr>
          <td align="right" valign="top">验证码</td>
          <td >
            <div style="padding-left:22px">
              <img src="captcha_code_file.php?rand=<?php echo rand();?>" id='captchaimg'><br>
              <label for='message'>请输入上面的字符</label>
              <br>
              <input id="captcha" name="captcha" type="text" size="50" class="required">
              <br>
              <a href='javascript: refreshCaptcha();'>看不清?换一张</a>              
            </div>

          </td>
        </tr>
        <tr>
          <td align="center" colspan="2">
            <input type="submit" value="注册" class="submit">            
          </td>
        </tr>
      </table>
    </form>       
  </div>  
  <script type='text/javascript'>
  function refreshCaptcha()
  {
  	var img = document.images['captchaimg'];
  	img.src = img.src.substring(0,img.src.lastIndexOf("?"))+"?rand="+Math.random()*1000;
  }

  </script> 
  
  <?php
  }
  ?>
</html>