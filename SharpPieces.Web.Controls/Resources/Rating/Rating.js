/*
Client class for Rating
*/


Rating = function (controlID, imageContainer, textContainer,valueContainer, allowMultipleChanges, messageList, ratingWidth)
{
   this._starWidth = -ratingWidth;
   this._controlID=controlID;
   this._imageContainer = imageContainer;
   this._textContainer = textContainer;
   this._valueContainer = valueContainer;
   this._ratingChanged = false;
   this._allowMultipleChanges = allowMultipleChanges;
   this._messageList = messageList;
      
   this.RestoreRating = function()
   {
        var container=document.getElementById(this._imageContainer);
        if(!parseInt(this.GetRatingValue()))
            this.SetRatingValue(0);
            
        container.style.backgroundPosition = "0px "+(parseInt(this.GetRatingValue())*this._starWidth)+"px";
        this.SetText(parseInt(this.GetRatingValue()));
   }
   
   this.SetText = function(index)
   {
        var textContainer = document.getElementById(this._textContainer);
        if(textContainer && this._messageList && this._messageList.length > index)
            textContainer.innerHTML = this._messageList[index];
   }
   
   this.GetRatingValue = function()
   {
        return document.getElementById(this._valueContainer).value;
   }
   
   this.SetRatingValue = function(value)
   {
        document.getElementById(this._valueContainer).value = value;
   }
  
   this.ChangeRating = function(ratingValue, persistentChange)
   {
        if(this._ratingChanged && !this._allowMultipleChanges)
            return;
            
        if(persistentChange)
        {
            this.SetRatingValue(ratingValue);
            this._ratingChanged = true;
        }
        
        var container=document.getElementById(this._imageContainer);
        container.style.backgroundPosition = "0px "+(ratingValue*this._starWidth)+"px";
        
        //if we have a text container and an array, also put some text...
        
        this.SetText(ratingValue);
        
   }
   
   this.RestoreRating();
}
