   M S B A M L   `   `   `  ����D  @crawlerUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:crawlerUI	crawlerUI  5   	       crawlerUI.RootWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:crawlerUI     5   	     � Title$  Root Window��5   	   $	��450��6   $	��450��6*    � 
ResizeMode$ CanMinimize=�66   .��5	      � .+�5
   
   �� -   5         ��Name$ 
tb_rootURL��5
      $
��Left=�6&   $��23��6A   $��
139,10,0,0q�6M    �� TextWrapping$
 Wrap=�6a   $	��Top=�6u   $	��287��6�    �� 
IsReadOnly . 6�   �� 5   
   -   5        $ tb_extActive��5      $
��Left=�6(   $��23��6C   $��
139,66,0,0q�6O   $
 Wrap=�6c   $	��Top=�6w   $	��287��6�    . 6�   �� 5   
   -   5        $ tb_maxThreadNo��5      $
��Left=�6*   $��23��6E   $��
139,38,0,0q�6Q   $
 Wrap=�6e   $	��Top=�6y   $	��287��6�    . 6�   �� 5   
   -   5         ��Name$ l_activelyRunningTasks��5      $��Actively Running Tasks ()��60   $
��Left=�6T   $��5,96,0,0q�6o   $	��Top=�6�   $��30��6�   $	��199��6�   $
��Bold"�6�    J� RenderTransformOrigin$ 0.482,-0.327&�6�   �� 5   
   $��	Root URL:��6   $
��Left=�6$   $��5,7,0,0q�6?   $	��Top=�6P   $ 0.538,0.276&�6h   $
��Bold"�6�   �� 5   
   $��Max. Thread Number:��6   $
��Left=�6.   $��5,35,0,0q�6I   $	��Top=�6[   $ 0.538,0.276&�6s   $
��Bold"�6�   �� 5   
   $��External Active:��6   $
��Left=�6+   $��5,65,0,0q�6F   $	��Top=�6X   $ 0.538,0.276&�6p   $
��Bold"�6�   �� 5   
   -   5         ��Name$ lv_activelyRunningTasks��5      $
��Left=�64   $	��284��6O   $��
10,126,0,0q�6\   $	��Top=�6p   $	��416��6�    �� View 5      �� 5      .P�5      �� 	 �� Width$		 130��6%   $��Task Status��61   
 �� DisplayMemberBinding
 6F   �� *Item2.Status+�� 5      $��Crawling URL��6%   
 6;   �� *Item3+5      5      5      5      