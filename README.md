实现功能点：
1、启动后最小化到托盘
	启动时，直接设置最小化，会出现问题，不稳定，有时会出现最小化不到托盘的尴尬场面。
	这里采取的方法是，第一次启动时，窗体设置成透明的，在shown事件中将窗体置为最小化，然后修改窗口透明度为不透明
2、web服务器创建

3、开机自动启动

4、最小化后在alt+tab中不再出现
