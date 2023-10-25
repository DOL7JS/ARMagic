<?php
     if ($_SERVER['REQUEST_METHOD'] === 'POST') { 
            if(isset($_POST['ConfigurationName']) && isset($_POST['ConfigurationContent']) && isset($_POST['folder'])){
                $myfile = fopen('./'.$_POST['folder'].'/'.$_POST['ConfigurationName'].'.json', "w") or die("Unable to open file!");
                fwrite($myfile, $_POST['ConfigurationContent']);
                fclose($myfile);
            }else if(isset($_FILES['model']) && isset($_POST['folder'])){
                $tmp_name = $_FILES['model']["tmp_name"];
                move_uploaded_file($tmp_name, $_POST['folder'].$_FILES['model']["name"]);
            }else{
                echo('Fields not set!');
            }
    }
?>
