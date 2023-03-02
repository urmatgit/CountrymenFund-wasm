using FSH.WebApi.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FSH.WebApi.Shared.Common;

public class MainPageModel
{
    public   const string NameJson = "HomePage.json";
    public CarouselModel CarouselModel { get; set; }
    public List<TextBlock> TextBlocs { get; set; }
    public MainPageModel()
    {
        TextBlocs= new List<TextBlock>();
    }
}
public class TextBlock
{
    public string Caption { get; set; }
    public string Text { get; set; }
    public string? TitleImage { get; set; } = "";
    public List<string> Images { get; set; } = new List<string>();
        
}
public class CarouselModel
{
    public bool AutoCycle { get; set; }
    public TimeSpan AutoCycleTime { get; set; } = TimeSpan.FromSeconds(5);
    public int Height { get; set; } = 200;
    
   public List<Slide> Slides { get; set; }
    public CarouselModel()
    {
        Slides= new List<Slide>();
    }
}

public enum SlideType {
    Text,
    Image
}
public class Slide
{
    
    //If  a slide type is an image, then value contain is a path to file
    public string? ImagePath { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public SlideTransition Transition { get; set; } = SlideTransition.Slide;

}
