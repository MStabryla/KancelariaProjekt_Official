export class BaseModel {
  constructor(obj?: any) {
    Object.assign(this, obj);
  }
  id: number =0;
}
