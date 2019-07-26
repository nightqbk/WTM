﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WalkingTec.Mvvm.TagHelpers.LayUI
{
    /// <summary>
    /// 滑块类型
    /// </summary>
    public enum SliderTypeEnum
    {
        // 水平滑块
        Default = 0,
        // 垂直滑块
        Vertical
    }

    /// <summary>
    /// 划块
    /// </summary>
    [HtmlTargetElement("wt:slider", Attributes = REQUIRED_ATTR_NAME1, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("wt:slider", Attributes = REQUIRED_ATTR_NAME, TagStructure = TagStructure.WithoutEndTag)]
    public class SliderTagHelper : BaseFieldTag
    {
        protected const string REQUIRED_ATTR_NAME1 = "field,[range=true],field1";
        private const string _idPrefix = "_slider";

        /// <summary>
        /// 绑定的字段 必填
        /// </summary>
        public ModelExpression Field1 { get; set; }

        /// <summary>
        /// 滑块类型，可选值有：default（水平滑块）、vertical（垂直滑块）
        /// 默认为 default（水平滑块）
        /// </summary>
        public SliderTypeEnum? SliderType { get; set; }

        /// <summary>
        /// 滑动条最小值，正整数，
        /// 默认为 0
        /// </summary>
        public int? Min { get; set; }

        /// <summary>
        /// 滑动条最大值，正整数，
        /// 默认为 100
        /// </summary>
        public int? Max { get; set; }

        /// <summary>
        /// 是否开启滑块的范围拖拽，若设为 true，则滑块将出现两个可拖拽的环，
        /// 默认false
        /// </summary>
        public bool? Range { get; set; }

        /// <summary>
        /// 滑块初始值，默认为数字
        /// 若开启了滑块为范围拖拽（即 range: true），则需赋值数组，异表示开始和结尾的区间，如：value: [30, 60]
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 拖动的步长
        /// 默认为正整数 1
        /// </summary>
        public uint Step { get; set; } = 1;

        /// <summary>
        /// 是否显示间断点
        /// 默认 false
        /// </summary>
        public bool ShowStep { get; set; }

        /// <summary>
        /// 是否显示文字提示
        /// 默认 true
        /// </summary>
        public bool Tips { get; set; } = true;

        /// <summary>
        /// 是否显示输入框（注意：若 range 参数为 true 则强制无效） 
        /// 点击输入框的上下按钮，以及输入任意数字后回车或失去焦点，均可动态改变滑块
        /// 默认 false
        /// </summary>
        public bool Input { get; set; }

        /// <summary>
        /// 滑动条高度，需配合 SliderType:Vertical 参数使用
        /// 默认 200
        /// </summary>
        public int? VSliderHeight { get; set; }

        /// <summary>
        /// 主题颜色，以便用在不同的主题风格下
        /// 默认 #009688
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// 数值改变的回调
        /// 在滑块数值被改变时触发。该回调非常重要，可动态获得滑块当前的数值。你可以将得到的数值，赋值给隐藏域，或者进行一些其它操作。
        /// param0: 当前值
        /// param1: 当前 slider 实例
        /// </summary>
        public string ChangeFunc { get; set; }

        /// <summary>
        /// 当鼠标放在圆点和滑块拖拽时均会触发提示层。
        /// 其默认显示的文本是它的对应数值，你也可以自定义提示内容
        /// param0: 当前值
        /// param1: 当前 slider 实例
        /// </summary>
        public string OnTipsFunc { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("id", $"{_idPrefix}{Id}");

            Range |= Field1 != null;

            string value0 = Field?.Model?.ToString();
            string value1 = Field1?.Model?.ToString();
            if (string.IsNullOrEmpty(Value))
            {
                value0 = Field?.Model == null ? "0" : Field.Model.ToString();
                value1 = Field1?.Model == null ? "0" : Field1.Model.ToString();
                Value = Range.HasValue && Range.Value ? $"[{value0},{value1}]" : value0;
            }
            else
            {
                if (Value.StartsWith('[') && Value.EndsWith(']'))
                {
                    var tmp = Value.TrimStart('[').TrimEnd(']').Replace(" ", string.Empty);
                    var arr = tmp.Split(",", StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x).ToArray();
                    value0 = arr[0];
                    value1 = arr[1];
                }
                else
                {
                    value0 = Value;
                }
            }

            var content = $@"
<input type='hidden' name='{Field.Name}' value='{value0}' class='layui-input'>
{(Field1 == null ? string.Empty : $"<input type='hidden' name='{Field1.Name}' value='{value1}' class='layui-input'>")}
<script>
;!function(){{
    var $ = layui.$;
    function defaultFunc(value,sliderIns) {{
        {(Range.HasValue && Range.Value ? $"$('input[name=\"{Field.Name}\"]').val(value[0]);$('input[name=\"{Field1.Name}\"]').val(value[1]);" : $"$('input[name=\"{Field.Name}\"]').val(value);")}
    }}
    var slider = layui.slider;
    var sliderIns = slider.render({{
        elem: '#{_idPrefix}{Id}'
        {(SliderType == null ? string.Empty : $",type:'{SliderType.Value.ToString().ToLower()}'")}
        {(Min == null ? string.Empty : $",min:{Min.Value}")}
        {(Max == null ? string.Empty : $",max:{Max.Value}")}
        {(Range == null ? string.Empty : $",range:{Range.Value.ToString().ToLower()}")}
        {(Value == null ? string.Empty : $",value:{Value}")}
        ,step:{Step}
        ,showstep:{ShowStep.ToString().ToLower()}
        ,tips:{Tips.ToString().ToLower()}
        ,input:{Input.ToString().ToLower()}
        {(SliderType == null || SliderType.Value == SliderTypeEnum.Default ? string.Empty : (VSliderHeight == null ? ",height:200" : $",height:{VSliderHeight.Value}"))}
        {(string.IsNullOrEmpty(Theme) ? string.Empty : $",theme: '{Theme}'")}
        ,change: function(value){{defaultFunc(value,sliderIns);
        {(string.IsNullOrEmpty(ChangeFunc) ? string.Empty : $"{ChangeFunc}(value,sliderIns)")}
        }}
        {(string.IsNullOrEmpty(OnTipsFunc) ? string.Empty : $",setTips: function(value){{{OnTipsFunc}(value,sliderIns);}}")}
    }});
    $('#{_idPrefix}{Id}').attr('style','min-height: 18px;padding-top: 18px;');
}}();
</script>
";
            output.PostElement.AppendHtml(content);
            base.Process(context, output);
        }
    }
}