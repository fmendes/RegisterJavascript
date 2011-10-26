/*
Client class for ImageCheckBox
*/


ImageCheckBox = function (controlID,regularImageUrl,checkedImageUrl,imageID,checkValueID, beforeChangeHandler, afterChangeHandler)
{
    this._controlID = controlID;
    this._regularImageUrl = regularImageUrl;
    this._checkedImageUrl = checkedImageUrl;
    this._imageID = imageID;
    this._checkValueID = checkValueID;
    
    this._before = beforeChangeHandler;
    this._after = afterChangeHandler;
    
    this.ChangeState = function ()
    {
        if(this._before)
        {
            if(typeof this._before != "function")
            {
                alert("BeforeChange function initialization error. Must be a function!");
                return;  
             }
             
             if(!this._before(this))
                return false;
        }
        
        var checkState = document.getElementById(this._checkValueID);
        if(checkState.value == "1")
        {
            checkState.value = "0";
            document.getElementById(this._imageID).src = this._regularImageUrl;
        }
        else
        {
            checkState.value = "1";
            document.getElementById(this._imageID).src = this._checkedImageUrl;
        }
        
        if(this._after)
        {
            if(typeof this._after != "function")
            {
                alert("AfterChange function initialization error. Must be a function!");
                return;  
             }
             
             !this._after(this);
        }
        
        return true;
    }
    
    this.CheckOnSpace = function(e)
    {
        var code;
        if (!e) var e = window.event
        if (e.keyCode) code = e.keyCode;
        else if (e.which) code = e.which;
        
        if(code == 32) //space pressed...
            this.ChangeState();
     }
}
