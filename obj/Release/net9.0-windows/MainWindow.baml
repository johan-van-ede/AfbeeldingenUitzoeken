   M S B A M L   `   `   `  ���� P  LAfbeeldingenUitzoeken, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:AfbeeldingenUitzoekenAfbeeldingenUitzoeken  &     AfbeeldingenUitzoeken.MainWindow   W SPresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5local#clr-namespace:AfbeeldingenUitzoeken       � Title$  Picture Sorting��$	��650��$
��1000�����#1E1E1E � 	Resources    DarkBackground �� Color  DarkSecondary  
DarkBorder  DarkText  GreenAccent  	RedAccent  
BlueAccent/   )AfbeeldingenUitzoeken.BrightnessConverter  BrightnessConverter   BrightnessFactor  UnifiedButtonStyle �� 
TargetType �� Property �� Value �� 
TargetType ��Name	 �� CornerRadius
 �� Triggers P� Property P� Value3  -System.Windows.Media.Effects.DropShadowEffect   Color   
BlurRadius   ShadowDepth   Opacity �� 
TargetName �� RelativeSource �� 	Converter �� ScaleX �� ScaleY 	 IconButtonStyle �� BasedOn 
 NavButtonStyle��%T  &	        &	      &	 &     &	 9     &	 L     &	 _     &	 r     &	 �     &	 �     7  &		 �    7  &	
 a    7  �� $ #1E1E1E���� $ #2D2D2D���� $ #3E3E3E���� $ #E0E0E0���� $ #5CB85C���� $ #D9534F���� $ #5BC0DE��  $	 1.2Q��� " ��.���  � �� �#3E3E3E��  � �� �#E0E0E0��  � ��$
 10,5q���  � ��$ 0q���  �@�Cursor$
 Hand����  � �� �� " ��.R��� $ border��#��z��$	 5��$��0q�._��� $��Center=�$��Center=�
 P�  �@J�IsMouseOver$
 True��. ���  �@J�Effect   $ Black��$ 10Q�$ 1Q�$	 0.7Q���  �@J�Opacity$	 0.9Q��� $ border�� �� *
Background+ �� *TemplatedParent+8    � ��P�  � ��$
 True��. ���  � z� �� $
 0.98Q�$
 0.98Q�P�  � {�$ False��. ���  �@J�Opacity$	 0.5Q��� " ��8   .���  � ��$ 50����  � ��$ 50����  � ��$ 8,5,8,5q���  � ��$ 22(��� " ��8   .���  � ��$ 40����  � ��$ 40����  �@J�Opacity$	 0.9Q���  � �� �#3E3E3E.��� #��[   � RowDefinitions �� $��* ��� $
��Auto � � ColumnDefinitions �� $	��250 ��� $��* �.+�� $��0��$��0�� �� $��* ��� $
��Auto �.+��� $��0��#��[ $��0,0,1,0q�$��	10,10,5,0q�._��� $
��Auto=�.���� ���� *PicturesQueue+���� .R��� #��[ $��0q�$��0q�$��5q�$	��158��$	��136������ * DataContext.SelectImageCommand+ �� *FindAncestor+ �� AncestorType" ������ .����  �� Padding$ 3q�#��[ #��[ $��1q�$	 3��$	��150��$��Center=�._��  �� $	��100 ��� $
��Auto �.+��� $��0��$��Uniform=�$	��100��$	��150��$��2q����� *	Thumbnail+�� $��1��$��CharacterEllipsis=�$��NoWrap=�$��10(�#��[ $��2q����� *
FileName+�� $��1��#��[ $��
10,5,10,10q�$ 10q�$	 3��._���  �� Orientation$ Vertical=�.+��� #��[ $��11(�$��Center=�$��0,2,0,2q����� *RemainingImageCount+ �� StringFormat$ Remaining Images: {0}���� #��[ $��11(�$��Center=�$��0,2,0,2q����� *ProcessedPercentage+$ Progress: {0}��� $��0��$��1�� �� $��* ��� $
��Auto �.+��� $��0��
���   �$��
10,10,10,0q�._��� $��Uniform=�$��0q����� *CurrentImage+�� $��1��#��[ $��
10,5,10,10q�$ 10q�$	 3��._��� $ Vertical=�$
��Left=�.+��� #��[ $
��Bold"�$��14(�$��0,0,0,5q����� *CurrentPicture.FileName+�� $ 
Horizontal=�$��0,2,0,2q�.+��� $��Date: ��#��[ $��12(��� #��[ $��12(����� *CurrentPicture.DateFormatted+�� $ 
Horizontal=�$��0,2,0,2q�.+��� $��Resolution: ��#��[ $��12(��� #��[ $��12(����� *$"CurrentPicture.ResolutionFormatted+�� $ 
Horizontal=�$��0,2,0,2q�.+��� $��File Size: ��#��[ $��12(��� #��[ $��12(����� *" CurrentPicture.FileSizeFormatted+�� $ 
Horizontal=�$��0,2,0,2q�.+��� $��Path: ��#��[ $��12(��� #��[ $��12(�$��CharacterEllipsis=����� *CurrentPicture.FilePath+�� $��0��#��[
 $
��Left=�$��Center=�$��20,0,0,0q����� *PreviousPictureCommand+{��� *CanGoPrevious+.���� $	��◀��$��16(�$��Center=�$��Center=� �� Padding$ 0,0,2,0q��� $��0��#��[
 $��Right=�$��Center=�$��0,0,20,0q����� *NextPictureCommand+{��� *	CanGoNext+.���� $	��▶��$��16(�$��Center=�$��Center=�$ 2,0,0,0q�� $��1��$��0��$��2��.+��� $ 
Horizontal=�$��Center=�$��5,2,5,2q�.+��� #��[	 #��[ $	��✓�� � ToolTip$
 Keep������ *KeepCommand+�� #��[	 #��[ $	��✗��$ 
Throw Away������ *ThrowAwayCommand+�� #��[	 #��[ $
��🕒��$ Check Later������ *CheckLaterCommand+�� -   #��[	 $��Right=�$��Center=�$��10q�$��40��$��40��#��[ $��⚙️��$ Settings��