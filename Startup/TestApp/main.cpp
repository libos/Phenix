#include <stdio.h>
#include <stdlib.h>
#include <string.h>
int main(int argc,char *argv[])
{ 
	int len = strlen(argv[1])+strlen(argv[2]);
	printf("result:(%s+%s):%d",argv[1],argv[2],len);
	return 0;
}
