import { Identity } from './Identity';

export class PropStorage
{
  private identities: { [id: string]: Identity; } = {};

  constructor()
  {

  }
  
  public setIdentity(url: string,  digest: string)
  {
  }
}
